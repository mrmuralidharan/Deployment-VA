// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using HDPagerSkill.Tests.Mocks;
using HDPagerSkill.Tests.Utterances;
using Luis;
using Microsoft.Bot.Builder;
using System.Collections.Generic;

namespace HDPagerSkill.Tests.Utilities
{
    public class SkillTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { SampleDialogUtterances.Trigger, CreateIntent(SampleDialogUtterances.Trigger, HdPagerSkillLuis.Intent.Sample) },
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, HdPagerSkillLuis.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static HdPagerSkillLuis CreateIntent(string userInput, HdPagerSkillLuis.Intent intent)
        {
            var result = new HdPagerSkillLuis
            {
                Text = userInput,
                Intents = new Dictionary<HdPagerSkillLuis.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new HdPagerSkillLuis._Entities
            {
                _instance = new HdPagerSkillLuis._Entities._Instance()
            };

            return result;
        }
    }
}
