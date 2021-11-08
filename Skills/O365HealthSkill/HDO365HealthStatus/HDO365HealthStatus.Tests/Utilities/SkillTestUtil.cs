// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using HDO365HealthStatus.Tests.Mocks;
using HDO365HealthStatus.Tests.Utterances;
using Luis;
using Microsoft.Bot.Builder;
using System.Collections.Generic;

namespace HDO365HealthStatus.Tests.Utilities
{
    public class SkillTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { SampleDialogUtterances.Trigger, CreateIntent(SampleDialogUtterances.Trigger, Hdo365HealthStatusLuis.Intent.Sample) },
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, Hdo365HealthStatusLuis.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static Hdo365HealthStatusLuis CreateIntent(string userInput, Hdo365HealthStatusLuis.Intent intent)
        {
            var result = new Hdo365HealthStatusLuis
            {
                Text = userInput,
                Intents = new Dictionary<Hdo365HealthStatusLuis.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new Hdo365HealthStatusLuis._Entities
            {
                _instance = new Hdo365HealthStatusLuis._Entities._Instance()
            };

            return result;
        }
    }
}
