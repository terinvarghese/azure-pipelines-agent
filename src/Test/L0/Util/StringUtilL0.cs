// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.VisualStudio.Services.Agent.Util;
using System;
using System.Text;
using System.Globalization;
using Xunit;

namespace Microsoft.VisualStudio.Services.Agent.Tests.Util
{
    public class StringUtilL0
    {
        [Theory]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        [InlineData("##vso[task.setvariable variable=testVar]a", "**vso[task.setvariable variable=testVar]a")]
        [InlineData("echo \"##vso[task.setvariable variable=testVar]a\"", "echo \"**vso[task.setvariable variable=testVar]a\"")]
        [InlineData("##vso a", "**vso a")]
        [InlineData("##vso[] a", "**vso[] a")]
        [InlineData("## vso", "## vso")]
        [InlineData("#vso", "#vso")]
        [InlineData("##vs", "##vs")]
        [InlineData("##VsO", "**vso")]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData(" ", "")]
        public void DeactivateVsoCommandsFromStringTest(string input, string expected)
        {
            var result = StringUtil.DeactivateVsoCommands(input);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// In this test, a vso command is encoded as a bse64 string and being passed as input to the DeactivateBase64EncodedVsoCommands
        /// The returned string when decoded will have ## replaced with **, deactivating the vso command.
        /// </summary>
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void DeactivateVsoCommandsIfBase64Encoded_EncodedVsoCommands_Returns_DeactivatedVsoCommands()
        {
            string vsoCommand = "##vso[task.setvariable variable=downloadUrl]https://www.evil.com";
            string encodedVsoCommand = Convert.ToBase64String(Encoding.UTF8.GetBytes(vsoCommand));

            string result = StringUtil.DeactivateVsoCommandsIfBase64Encoded(encodedVsoCommand);

            string deactivatedVsoCommand = "**vso[task.setvariable variable=downloadUrl]https://www.evil.com";
            var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes(deactivatedVsoCommand));

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// In this test, a vso command is being passed as input to the DeactivateBase64EncodedVsoCommands
        /// The unmodified string would be returned.
        /// </summary>
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void DeactivateVsoCommandsIfBase64Encoded_NotEncodedVsoCommands_Throws_FormatException()
        {
            string vsoCommand = "##vso[task.setvariable variable=downloadUrl]https://www.evil.com";
            Assert.Throws<FormatException>(() => StringUtil.DeactivateVsoCommandsIfBase64Encoded(vsoCommand));
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void DeactivateVsoCommandsIfBase64Encoded_InputEmpty_Returns_UnmodifiedString()
        {
            string vsoCommand = "";
            string result = StringUtil.DeactivateVsoCommandsIfBase64Encoded(vsoCommand);
            Assert.Equal(vsoCommand, result);
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void DeactivateVsoCommandsIfBase64Encoded_InputNull_Throws_Exception()
        {
            string vsoCommand = null;
            Assert.Throws<ArgumentNullException>(() => StringUtil.DeactivateVsoCommandsIfBase64Encoded(vsoCommand));
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void FormatAlwaysCallsFormat()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace();

                // Arrange.
                var variableSets = new[]
                {
                    new { Format = null as string, Args = null as object[], Expected = string.Empty },
                    new { Format = null as string, Args = new object[0], Expected = string.Empty },
                    new { Format = null as string, Args = new object[] { 123 }, Expected = string.Empty },
                    new { Format = "Some message", Args = null as object[], Expected = "Some message" },
                    new { Format = "Some message", Args = new object[0], Expected = "Some message" },
                    new { Format = "Some message", Args = new object[] { 123 }, Expected = "Some message" },
                    new { Format = "Some format '{0}'", Args = null as object[], Expected = "Some format ''" },
                    new { Format = "Some format '{0}'", Args = new object[0], Expected = "Some format ''" },
                    new { Format = "Some format '{0}'", Args = new object[] { 123 }, Expected = "Some format '123'" },
                };
                foreach (var variableSet in variableSets)
                {
                    trace.Info($"{nameof(variableSet)}:");
                    trace.Info(variableSet);

                    // Act.
                    string actual = StringUtil.Format(variableSet.Format, variableSet.Args);

                    // Assert.
                    Assert.Equal(variableSet.Expected, actual);
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void FormatHandlesFormatException()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace();

                // Arrange.
                var variableSets = new[]
                {
                    new { Format = "Bad format { 0}", Args = null as object[], Expected = "Bad format { 0}" },
                    new { Format = "Bad format { 0}", Args = new object[0], Expected = "Bad format { 0} " },
                    new { Format = "Bad format { 0}", Args = new object[] { null }, Expected = "Bad format { 0} " },
                    new { Format = "Bad format { 0}", Args = new object[] { 123, 456 }, Expected = "Bad format { 0} 123, 456" },
                };
                foreach (var variableSet in variableSets)
                {
                    trace.Info($"{nameof(variableSet)}:");
                    trace.Info(variableSet);

                    // Act.
                    string actual = StringUtil.Format(variableSet.Format, variableSet.Args);

                    // Assert.
                    Assert.Equal(variableSet.Expected, actual);
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void FormatUsesInvariantCulture()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                // Arrange.
                CultureInfo originalCulture = CultureInfo.CurrentCulture;
                try
                {
                    CultureInfo.CurrentCulture = new CultureInfo("it-IT");

                    // Act.
                    string actual = StringUtil.Format("{0:N2}", 123456.789);

                    // Actual
                    Assert.Equal("123,456.79", actual);
                }
                finally
                {
                    CultureInfo.CurrentCulture = originalCulture;
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ConvertNullOrEmptryStringToBool()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                // Arrange.
                string nullString = null;
                string emptyString = string.Empty;

                // Act.
                bool result1 = StringUtil.ConvertToBoolean(nullString);
                bool result2 = StringUtil.ConvertToBoolean(emptyString);

                // Actual
                Assert.False(result1, "Null String should convert to false.");
                Assert.False(result2, "Empty String should convert to false.");
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ConvertNullOrEmptryStringToDefaultBool()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                // Arrange.
                string nullString = null;
                string emptyString = string.Empty;

                // Act.
                bool result1 = StringUtil.ConvertToBoolean(nullString, true);
                bool result2 = StringUtil.ConvertToBoolean(emptyString, true);

                // Actual
                Assert.True(result1, "Null String should convert to true since default value is set to true.");
                Assert.True(result2, "Empty String should convert to true since default value is set to true.");
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ConvertStringToBool()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                // Arrange.
                string trueString1 = "1";
                string trueString2 = "True";
                string trueString3 = "$TRUE";
                string falseString1 = "0";
                string falseString2 = "false";
                string falseString3 = "$False";

                string undefineString1 = "-1";
                string undefineString2 = "sometext";
                string undefineString3 = "2015-03-21";

                // Act.
                bool result1 = StringUtil.ConvertToBoolean(trueString1, false);
                bool result2 = StringUtil.ConvertToBoolean(trueString2);
                bool result3 = StringUtil.ConvertToBoolean(trueString3, true);
                bool result4 = StringUtil.ConvertToBoolean(falseString1, true);
                bool result5 = StringUtil.ConvertToBoolean(falseString2);
                bool result6 = StringUtil.ConvertToBoolean(falseString3, false);

                bool result7 = StringUtil.ConvertToBoolean(undefineString1, true);
                bool result8 = StringUtil.ConvertToBoolean(undefineString2);
                bool result9 = StringUtil.ConvertToBoolean(undefineString3, false);

                // Actual
                Assert.True(result1, $"'{trueString1}' should convert to true.");
                Assert.True(result2, $"'{trueString2}' should convert to true.");
                Assert.True(result3, $"'{trueString3}' should convert to true.");
                Assert.False(result4, $"'{falseString1}' should convert to false.");
                Assert.False(result5, $"'{falseString2}' should convert to false.");
                Assert.False(result6, $"'{falseString3}' should convert to false.");

                Assert.True(result7, $"'{undefineString1}' should convert to true, since default is true.");
                Assert.False(result8, $"'{undefineString2}' should convert to false.");
                Assert.False(result9, $"'{undefineString3}' should convert to false.");
            }
        }
    }
}
