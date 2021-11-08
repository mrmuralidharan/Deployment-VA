// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Luis;

namespace DepartmentSkill.Models
{
    public class SkillState
    {
        public SkillState()
        {
            Clear();
        }
        public string Token { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public DepartmentSkillLuis LuisResult { get; internal set; }

        public string DeptDetail { get; set; }
        public bool IsAction { get; set; }

        public void Clear()
        {
            //Clears the state values
        }
    }
}
