using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

// Reflection lets an isolated test assembly exercise the existing Assembly-CSharp
// without moving the game's scripts into a new assembly.
public class TurretTargetingTests
{
    private readonly List<GameObject> objects = new List<GameObject>();
    private Component turret;
    private Type enemyType;
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    [SetUp]
    public void SetUp()
    {
        Assembly game = Assembly.Load("Assembly-CSharp");
        enemyType = game.GetType("Enemy", true);
        turret = CreateObject("Turret").AddComponent(game.GetType("TurretObject", true));
        Set(turret, "currentRateOfFire", -100f);
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = objects.Count - 1; i >= 0; i--)
            if (objects[i] != null) Object.DestroyImmediate(objects[i]);
        objects.Clear();
    }

    private GameObject CreateObject(string name)
    {
        var obj = new GameObject(name);
        objects.Add(obj);
        return obj;
    }

    private Component CreateEnemy(float x)
    {
        GameObject obj = CreateObject("Enemy");
        obj.tag = "Enemy";
        obj.transform.position = new Vector3(x, 0, 0);
        Component enemy = obj.AddComponent(enemyType);
        Set(enemy, "enemyBody", obj.transform);
        obj.AddComponent<BoxCollider>();
        Enter(enemy.GetComponent<Collider>());
        return enemy;
    }

    private static void Set(Component component, string field, object value) =>
        component.GetType().GetField(field, Fields).SetValue(component, value);
    private object Get(string field) => turret.GetType().GetField(field, Fields).GetValue(turret);
    private void Call(string method, params object[] args) =>
        turret.GetType().GetMethod(method, Fields).Invoke(turret, args);
    private void Enter(Collider collider) => Call("OnTriggerEnter", collider);
    private int Count => (int)Get("currentQueueCount");
    private Component Top => (Component)((Array)Get("priorityQueue")).GetValue(0);

    private void AssertHeap()
    {
        Array queue = (Array)Get("priorityQueue");
        for (int i = 1; i < Count; i++)
        {
            Component child = (Component)queue.GetValue(i);
            Component parent = (Component)queue.GetValue((i - 1) / 2);
            Assert.LessOrEqual((parent.transform.position - turret.transform.position).sqrMagnitude,
                (child.transform.position - turret.transform.position).sqrMagnitude);
        }
    }

    [Test]
    public void AttackIncludesNewArrivalsAndSwitchesToNearestWithoutDequeuing()
    {
        CreateEnemy(5);
        Call("Update"); // Idle -> Attack
        Component closer = CreateEnemy(2);
        Call("Update");
        Assert.AreEqual(2, Count);
        Assert.AreEqual(closer.gameObject, Get("attackTarget"));
    }

    [Test]
    public void SwitchingTargetsPreservesAccumulatedCooldown()
    {
        CreateEnemy(5);
        Call("Update");
        Set(turret, "currentRateOfFire", 0.25f);
        Component closer = CreateEnemy(2);
        Call("Update");
        Assert.AreEqual(closer.gameObject, Get("attackTarget"));
        Assert.AreEqual(0.25f + Time.deltaTime, (float)Get("currentRateOfFire"), 0.00001f);
    }

    [Test]
    public void MovingEnemiesAndTurretReorderBothHeapChildren()
    {
        Component a = CreateEnemy(1);
        Component b = CreateEnemy(2);
        Component c = CreateEnemy(3);
        a.transform.position = new Vector3(9, 0, 0);
        c.transform.position = new Vector3(0.5f, 0, 0);
        Call("RefreshPriorityQueue");
        Assert.AreEqual(c, Top); // Right child must beat left child.
        AssertHeap();
        turret.transform.position = new Vector3(8, 0, 0);
        Call("RefreshPriorityQueue");
        Assert.AreEqual(a, Top);
        AssertHeap();
        b.transform.position = turret.transform.position;
        Call("RefreshPriorityQueue");
        Assert.AreEqual(b, Top);
        AssertHeap();
    }

    [Test]
    public void DuplicateCollidersAndDefaultEnemyIdsDoNotLoseCandidates()
    {
        Component a = CreateEnemy(1);
        CreateEnemy(2); // Both EnemyID values are zero.
        Collider first = a.GetComponent<Collider>();
        Enter(first);
        GameObject child = CreateObject("Child collider");
        child.transform.SetParent(a.transform);
        Collider second = child.AddComponent<BoxCollider>();
        Enter(second);
        Assert.AreEqual(2, Count);
        Call("OnTriggerExit", first);
        Assert.AreEqual(2, Count);
        Call("OnTriggerExit", second);
        Assert.AreEqual(1, Count);
    }

    [Test]
    public void ExitRemovesLastAndOnlyCandidateAndClearsTarget()
    {
        Component near = CreateEnemy(1);
        Component far = CreateEnemy(2);
        Call("OnTriggerExit", far.GetComponent<Collider>());
        Assert.AreEqual(1, Count);
        Call("Update");
        Call("OnTriggerExit", near.GetComponent<Collider>());
        Call("Update");
        Assert.AreEqual(0, Count);
        Assert.IsNull(Get("attackTarget"));
        Assert.AreEqual("Idle", Get("turretState").ToString());
    }

    [Test]
    public void LethalHealthAndDestroyedRootPreserveOtherCandidates()
    {
        Component dead = CreateEnemy(1);
        Component destroyed = CreateEnemy(2);
        Component survivor = CreateEnemy(3);
        Set(dead, "currentHP", 0f);
        Object.DestroyImmediate(destroyed.gameObject);
        Call("RefreshPriorityQueue");
        Assert.AreEqual(1, Count);
        Assert.AreEqual(survivor, Top);
        Enter(dead.GetComponent<Collider>());
        Assert.AreEqual(1, Count);
    }

    [Test]
    public void DeathEventImmediatelyRemovesCandidateAndUnsubscribes()
    {
        Component enemy = CreateEnemy(1);
        var field = enemyType.GetField("OnEnemyDied", Fields);
        ((Delegate)field.GetValue(enemy)).DynamicInvoke(enemy);
        Assert.AreEqual(0, Count);
        Assert.IsNull(field.GetValue(enemy));
    }

    [Test]
    public void DisabledColliderIsPrunedAndStayCanRestoreIt()
    {
        Component enemy = CreateEnemy(1);
        Collider collider = enemy.GetComponent<Collider>();
        collider.enabled = false;
        Call("RefreshPriorityQueue");
        Assert.AreEqual(0, Count);
        collider.enabled = true;
        Call("OnTriggerStay", collider);
        Assert.AreEqual(1, Count);
        ((Behaviour)turret).enabled = false;
        // EditMode does not run ordinary MonoBehaviour lifecycle callbacks.
        Call("OnDisable");
        Assert.AreEqual(0, Count);
        Assert.IsNull(enemyType.GetField("OnEnemyDied", Fields).GetValue(enemy));
    }

    [Test]
    public void GrowthAndRepeatedMovementAgreeWithLinearNearestSearch()
    {
        var enemies = new List<Component>();
        var random = new System.Random(42);
        for (int i = 0; i < 75; i++) enemies.Add(CreateEnemy(i + 1));
        for (int round = 0; round < 20; round++)
        {
            Component expected = null;
            float nearest = float.PositiveInfinity;
            foreach (Component enemy in enemies)
            {
                enemy.transform.position = new Vector3((float)random.NextDouble() * 100, 0, 0);
                float distance = enemy.transform.position.sqrMagnitude;
                if (distance < nearest) { nearest = distance; expected = enemy; }
            }
            Call("RefreshPriorityQueue");
            Assert.AreEqual(75, Count);
            Assert.AreEqual(expected, Top);
            AssertHeap();
        }
    }
}
