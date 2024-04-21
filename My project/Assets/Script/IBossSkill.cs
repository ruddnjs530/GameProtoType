using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IBossSkill
{
    bool IsActive { get; set; }
    bool CanActivate();
    void Activate();
    event Action OnSkillFinished;
}
