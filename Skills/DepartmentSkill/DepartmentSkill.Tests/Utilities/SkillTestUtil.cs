// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DepartmentSkill.Tests.Mocks;
using DepartmentSkill.Tests.Utterances;
using Luis;
using Microsoft.Bot.Builder;
using System.Collections.Generic;

namespace DepartmentSkill.Tests.Utilities
{
    public class SkillTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { SampleDialogUtterances.Trigger, CreateIntent(SampleDialogUtterances.Trigger, DepartmentSkillLuis.Intent.Sample) },
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, DepartmentSkillLuis.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static DepartmentSkillLuis CreateIntent(string userInput, DepartmentSkillLuis.Intent intent)
        {
            var result = new DepartmentSkillLuis
            {
                Text = userInput,
                Intents = new Dictionary<DepartmentSkillLuis.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new DepartmentSkillLuis._Entities
            {
                _instance = new DepartmentSkillLuis._Entities._Instance()
            };

            return result;
        }
    }
}
