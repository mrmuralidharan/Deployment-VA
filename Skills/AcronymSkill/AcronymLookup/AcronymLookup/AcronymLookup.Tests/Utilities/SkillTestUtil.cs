// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AcronymLookup.Tests.Mocks;
using AcronymLookup.Tests.Utterances;
using Luis;
using Microsoft.Bot.Builder;
using System.Collections.Generic;

namespace AcronymLookup.Tests.Utilities
{
    public class SkillTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { SampleDialogUtterances.Trigger, CreateIntent(SampleDialogUtterances.Trigger, AcronymLookupLuis.Intent.Sample) },
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, AcronymLookupLuis.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static AcronymLookupLuis CreateIntent(string userInput, AcronymLookupLuis.Intent intent)
        {
            var result = new AcronymLookupLuis
            {
                Text = userInput,
                Intents = new Dictionary<AcronymLookupLuis.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new AcronymLookupLuis._Entities
            {
                _instance = new AcronymLookupLuis._Entities._Instance()
            };

            return result;
        }
    }
}
