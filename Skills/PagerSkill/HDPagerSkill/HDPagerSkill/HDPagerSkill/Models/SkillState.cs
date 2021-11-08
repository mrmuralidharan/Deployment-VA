// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace HDPagerSkill.Models
{
    public class SkillState
    {
        public string Token { get; set; }

        public TimeZoneInfo TimeZone { get; set; }
        public Boolean IsAction { get; set; }
        public string apiResponseString { get; set; }

        public void Clear()
        {
        }
    }
}
