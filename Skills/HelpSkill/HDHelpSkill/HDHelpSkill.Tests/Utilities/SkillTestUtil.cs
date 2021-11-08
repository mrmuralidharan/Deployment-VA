// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using HDHelpSkill.Tests.Mocks;
using HDHelpSkill.Tests.Utterances;
using Luis;
using Microsoft.Bot.Builder;
using System.Collections.Generic;

namespace HDHelpSkill.Tests.Utilities
{
    public class SkillTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { SampleDialogUtterances.Trigger, CreateIntent(SampleDialogUtterances.Trigger, HdHelpSkillLuis.Intent.Sample) },
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, HdHelpSkillLuis.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static HdHelpSkillLuis CreateIntent(string userInput, HdHelpSkillLuis.Intent intent)
        {
            var result = new HdHelpSkillLuis
            {
                Text = userInput,
                Intents = new Dictionary<HdHelpSkillLuis.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new HdHelpSkillLuis._Entities
            {
                _instance = new HdHelpSkillLuis._Entities._Instance()
            };

            return result;
        }
    }
}
