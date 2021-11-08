﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace HDHelpSkill.Models
{
    public class SkillState
    {
        public string Token { get; set; }

        public TimeZoneInfo TimeZone { get; set; }
        public bool IsAction { get; internal set; }

        public void Clear()
        {
        }
    }
}
