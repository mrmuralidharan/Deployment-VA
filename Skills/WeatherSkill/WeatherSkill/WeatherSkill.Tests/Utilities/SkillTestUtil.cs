// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Luis;
using Microsoft.Bot.Builder;
using System.Collections.Generic;
using WeatherSkill.Tests.Mocks;
using WeatherSkill.Tests.Utterances;

namespace WeatherSkill.Tests.Utilities
{
    public class SkillTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { SampleDialogUtterances.Trigger, CreateIntent(SampleDialogUtterances.Trigger, WeatherSkillLuis.Intent.Sample) },
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, WeatherSkillLuis.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static WeatherSkillLuis CreateIntent(string userInput, WeatherSkillLuis.Intent intent)
        {
            var result = new WeatherSkillLuis
            {
                Text = userInput,
                Intents = new Dictionary<WeatherSkillLuis.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new WeatherSkillLuis._Entities
            {
                _instance = new WeatherSkillLuis._Entities._Instance()
            };

            return result;
        }
    }
}
