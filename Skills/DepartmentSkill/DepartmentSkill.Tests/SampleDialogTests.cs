// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DepartmentSkill.Tests.Utterances;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DepartmentSkill.Tests
{
    [TestClass]
    public class SampleDialogTests : SkillTestBase
    {
        [TestMethod]
        public async Task Test_Sample_Dialog()
        {
            await GetTestFlow()
                .Send(SampleDialogUtterances.Trigger)
                .AssertReplyOneOf(GetTemplates("FirstPromptText"))
                .Send(SampleDialogUtterances.Trigger)
                .AssertReplyOneOf(GetTemplates("NamePromptText"))
                .Send(SampleDialogUtterances.NamePromptResponse)
                .AssertReplyOneOf(GetTemplates("HaveNameMessageText", new { Name = SampleDialogUtterances.NamePromptResponse }))
                .StartTestAsync();
        }
    }
}
