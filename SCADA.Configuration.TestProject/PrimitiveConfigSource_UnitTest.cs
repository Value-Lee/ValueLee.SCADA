using System;
using System.IO;
using System.Text;
using Xunit;

namespace SCADA.Configuration.TestProject
{
    public class PrimitiveConfigSource_UnitTest
    {
        #region Validate

        [Fact]
        public void IsValidFile_Test()
        {
            Assert.True(Parser.TryParse2File("C:\\Test\\test.txt", out var _));
            Assert.True(Parser.TryParse2File("D:\\Test\\test.txt", out var _));
            Assert.True(Parser.TryParse2File("Z:\\Test\\test.txt\\..", out var _));
            Assert.True(Parser.TryParse2File("C:\\Test\\..\\test.txt", out var _));
            Assert.True(Parser.TryParse2File("C:\\Test\\test.txt\\#", out var _));
            Assert.False(Parser.TryParse2File("D:\\Test\\\\test.txt", out var _));
            Assert.False(Parser.TryParse2File("C:\\Test\\test.txt\\?", out var _));
            Assert.False(Parser.TryParse2File("C:\\Test\\test*?.txt", out var _));
            Assert.False(Parser.TryParse2File("C:\\Test\\test.txt\\", out var _));
            Assert.False(Parser.TryParse2File("C:\\Test\\\\test.txt", out var _));
            Assert.False(Parser.TryParse2File("C:\\Test\\test.txt/", out var _));
            Assert.False(Parser.TryParse2File("C:\\Test\\test.txt?", out var _));
            Assert.False(Parser.TryParse2File("C:\\Test\\test.txt*", out var _));
            Assert.False(Parser.TryParse2File("C:\\", out var _));
        }

        [Fact]
        public void IsValidFolder_Test()
        {
            Assert.True(Parser.TryParse2Directory("C:\\Test\\", out var _));
            Assert.True(Parser.TryParse2Directory("D:\\Test\\", out var _));
            Assert.True(Parser.TryParse2Directory("Z:\\Test\\..\\", out var _));
            Assert.True(Parser.TryParse2Directory("C:\\Test\\..\\", out var _));
            Assert.True(Parser.TryParse2Directory("C:\\Test\\#", out var _));
            Assert.True(Parser.TryParse2Directory("C:\\", out var _));
            Assert.False(Parser.TryParse2Directory("D:\\\\Test\\", out var _));
            Assert.False(Parser.TryParse2Directory("C:\\Test\\?", out var _));
            Assert.False(Parser.TryParse2Directory("C:\\Test\\*", out var _));
            Assert.False(Parser.TryParse2Directory("C:\\Test\\test.txt/", out var _));
            Assert.False(Parser.TryParse2Directory("C:\\Test\\\\", out var _));
            Assert.False(Parser.TryParse2Directory("C:\\Test\\test.txt?", out var _));
            Assert.False(Parser.TryParse2Directory("C:\\Test\\test.txt*", out var _));
        }

        [Fact]
        public void IsValidColor_Test()
        {
            Assert.True(Parser.TryParse2Color("#FFFFFF", out var _));
            Assert.True(Parser.TryParse2Color("#A0FFFFFF", out var _));
            Assert.False(Parser.TryParse2Color("#FFFFF", out var _)); // Invalid length
            Assert.False(Parser.TryParse2Color("A0FFFFFF", out var _)); // Missing '#'
            Assert.False(Parser.TryParse2Color("#GHIJKL", out var _)); // Invalid hex characters
            Assert.False(Parser.TryParse2Color("#12345G", out var _)); // Invalid hex characters
            Assert.False(Parser.TryParse2Color("123456", out var _)); // Missing '#'
        }

        #endregion Validate

        #region GetValue

        [Fact]
        public void GetValue_Number_Test()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <root>
                	<config name=""System"">
                		<config name=""Count1"" value=""10"" type=""Integer"" />
                		<config name=""Count2"" value=""0x0A"" type=""Integer"" />
                        <config name=""Count3"" value=""-1,000"" type=""Integer"" />
                        <config name=""Count4"" value=""10,00,00"" type=""Integer"" />
                        <config name=""Count5"" value=""1.00"" type=""Integer"" />
                        <config name=""Count6"" value=""12,345.00"" type=""Integer"" />

                        <config name=""Number1"" value=""6.5"" type=""Decimal"" />
                        <config name=""Number2"" value=""6"" type=""Decimal"" />
                        <config name=""Number3"" value=""-6.5"" type=""Decimal"" />
                        <config name=""Number4"" value=""1,234.5"" type=""Decimal"" />
                        <config name=""Number5"" value=""0x0A"" type=""Decimal"" />
                        <config name=""Number6"" value=""1,234.5e-3"" type=""Decimal"" />
                	</config>
                </root>";


            using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml))
            {
                // Test Integer
                Assert.Equal(10, source.GetValue<int>("System.Count1"));
                Assert.Equal(10, source.GetValue<double>("System.Count1")); // Integer supports mapping to C# decimal
                Assert.Equal(10, source.GetValue<int>("System.Count2"));
                Assert.Equal(-1000, source.GetValue<int>("System.Count3"));
                Assert.Equal(100000, source.GetValue<int>("System.Count4"));
                Assert.Equal(1, source.GetValue<int>("System.Count5")); // It is allowed to be a decimal with all decimal places being zeros
                Assert.Equal(12345, source.GetValue<int>("System.Count6"));
                // Test Decimal
                Assert.Equal(6.5, source.GetValue<double>("System.Number1"));
                Assert.Equal(6, source.GetValue<double>("System.Number2"));
                Assert.Equal(-6.5, source.GetValue<double>("System.Number3"));
                Assert.Equal(1234.5, source.GetValue<double>("System.Number4"));
                Assert.Equal(10, source.GetValue<double>("System.Number5"));
                Assert.Equal(1.2345, source.GetValue<double>("System.Number6")); // Support for scientific notation.
                Assert.ThrowsAny<Exception>(() => source.GetValue<long>("System.Number2")); // Decimal does not support mapping to C# integers
            }
        }

        [Fact]
        public void GetValue_File_Test()
        {

            string xml1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""DataReport"" value=""C:\data.csv"" type=""File""/>
	                            </config>
                            </root>";
            using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml1))
            {
                var file = source.GetValue<FileInfo>("System.DataReport");
                Assert.Equal("C:\\data.csv", file.FullName);
                Assert.ThrowsAny<Exception>(() => source.GetValue<DirectoryInfo>("System.DataReport")); // File cannot be converted to DirectoryInfo
            }
                
        }

        [Fact]
        public void GetValue_Folder_Test()
        {
            string xml1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""LogsFolder"" value=""D:\Logs"" type=""Folder""/>
	                            </config>
                            </root>";
            using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml1))
            {
                var folder = source.GetValue<DirectoryInfo>("System.LogsFolder");
                Assert.Equal("D:\\Logs", folder.FullName);
                Assert.ThrowsAny<Exception>(() => source.GetValue<FileInfo>("System.LogsFolder")); // File cannot be converted to DirectoryInfo
            }
               
        }

        [Fact]
        public void GetValue_DateTime_Test()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <root>
	                        <config name=""System"">
		                        <config name=""ResetDate1"" value=""2020-02-02 02:02:02"" type=""DateTime""/>
                                <config name=""ResetDate2"" value=""2020-02-02"" type=""DateTime""/>
                                <config name=""ResetDate3"" value=""02:02:02"" type=""DateTime""/>
                                <config name=""ResetDate4"" value=""2020/2/2"" type=""DateTime""/>
                                <config name=""ResetDate5"" value=""2020/02/02"" type=""DateTime""/>
                                <config name=""ResetDate6"" value=""02:02"" type=""DateTime""/>
	                        </config>
                        </root>";
            using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml))
            {
                source.GetValue<DateTime>("System.ResetDate1");
                source.GetValue<DateTime>("System.ResetDate2");
                source.GetValue<DateTime>("System.ResetDate3");
                source.GetValue<DateTime>("System.ResetDate4");
                source.GetValue<DateTime>("System.ResetDate5");
                source.GetValue<DateTime>("System.ResetDate6");
            }

        }

        [Fact]
        public void GetValue_Color_Test()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""Color1"" value=""#FFFFFF"" type=""Color""/>
                                    <config name=""Color2"" value=""#A0FFFFFF"" type=""Color""/>
	                            </config>
                            </root>";
            using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml))
            {
                source.GetValue<System.Drawing.Color>("System.Color1");
                source.GetValue<System.Drawing.Color>("System.Color2");
            }

        }

#endregion GetValue

        [Fact]
        public void Find_Test()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""CycleCount"" value=""1"" type=""Integer"" options=""1;02;0x03""/>
                                    <config name=""DelayTime"" value=""03"" type=""Decimal"" options=""1;02;0x03;11,33e2;-3.14""/>
	                            </config>
                                <config name=""SetUp"">
                                    <config name=""Address"">
                                        <config name=""RemoteIpAddress"" value=""127.0.0.3"" type=""String"" options=""127.0.0.2;127.0.0.3;127.0.0.4""/>
                                    </config>
	                            </config>
                            </root>";
            using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml))
            {
                var ret = ConfigNode.Find("System", false, source.Nodes[0], out ConfigItem configItem, out ConfigNode configNode);
                Assert.True(ret);
                Assert.NotNull(configNode);
                Assert.Null(configItem);
                Assert.Equal("System", configNode.Name);

                ret = ConfigNode.Find("System", true, source.Nodes[0], out configItem, out configNode);
                Assert.False(ret);
                Assert.Null(configNode);
                Assert.Null(configItem);

                ret = ConfigNode.Find("System.DelayTime", true, source.Nodes[0], out configItem, out configNode);
                Assert.True(ret);
                Assert.NotNull(configNode);
                Assert.NotNull(configItem);
                Assert.Equal("System", configNode.Name);
                Assert.Equal("DelayTime", configItem.Name);

                ret = ConfigNode.Find("Address.RemoteIpAddress", true, source.Nodes[1].Children[0], out configItem, out configNode);
                Assert.True(ret);
                Assert.NotNull(configNode);
                Assert.NotNull(configItem);
                Assert.Equal("Address", configNode.Name);
                Assert.Equal("RemoteIpAddress", configItem.Name);

                ret = ConfigNode.Find("Address.RemoteIp.Address", true, source.Nodes[1].Children[0], out configItem, out configNode);
                Assert.False(ret);
                Assert.Null(configNode);
                Assert.Null(configItem);
            }
        }



        [Fact]
        public void NameCannotContainDot_Test()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <root>
	                        <config name=""Sys.tem"">
		                        <config name=""Email"" value=""0X06"" type=""Integer"" />
                                <config name=""Email2"" value=""xuyue@qq.com"" type=""string""  />
	                        </config>
                        </root>";

            Assert.ThrowsAny<Exception>(() =>
            {
                using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml)) { }
            });

            string xml2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""Emai.l"" value=""0X06"" type=""Integer"" />
                                    <config name=""Email2"" value=""xuyue@qq.com"" type=""string""  />
	                            </config>
                            </root>";

            Assert.ThrowsAny<Exception>(() =>
            {
                using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml2)) { }
            });

            string xml3 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""Em.ail""></config>
	                            </config>
                            </root>";

            Assert.ThrowsAny<Exception>(() =>
            {
                using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml3)) { }
            });
        }

        [Fact]
        public void NameCannotDuplicated()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""Email"" value=""0X06"" type=""Integer""/>
	                            </config>
	                            <config name=""System"">
		                            <config name=""Email2"" value=""0X06"" type=""Integer""/>
	                            </config>
                            </root>";
            Assert.ThrowsAny<Exception>(() =>
            {
                using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml)) { }
            });

            string xml2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""Email"" value=""0X06"" type=""Integer""/>
	                                <config name=""Email"">
		                                <config name=""Email"" value=""0X06"" type=""Integer""/>
	                                </config>
	                            </config>
                            </root>";
            using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml2)) { }

            string xml3 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""Count"" value=""0X06"" type=""Integer""/>
                                    <config name=""Count"" value=""0X07"" type=""Integer""/>
	                            </config>
                            </root>";
            Assert.ThrowsAny<Exception>(() =>
            {
                using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml3)) { }
            });

            string xml4 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""Count"" value=""0X06"" type=""Integer""/>
                                    <config name=""Count"">
                                        <config name=""Email"" value=""0X06"" type=""Integer""/>
                                    </config>
	                            </config>
                            </root>";
            using (PrimitiveConfigSource source2 = new PrimitiveConfigSource(xml4))
            {

            }

            string xml5 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
                                    <config name=""Count"">
                                        <config name=""Email"" value=""0X06"" type=""Integer""/>
                                        <config name=""Email"" value=""0X07"" type=""Integer""/>
                                    </config>
	                            </config>
                            </root>";
            Assert.ThrowsAny<Exception>(() =>
            {
                using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml5))
                {

                }
            });

            string xml6 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
                                    <config name=""Count"">
                                        <config name=""Email""></config>
                                        <config name=""Email""></config>
                                    </config>
	                            </config>
                            </root>";
            Assert.ThrowsAny<Exception>(() =>
            {
                using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml6))
                {

                }
            });
        }

        [Fact]
        public void RestoreDefaultConfigsOnAppRestart_Test()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""CycleCount"" value=""3"" type=""Integer""/>
	                            </config>
                                <config name=""SetUp"">
                                    <config name=""Address"">
                                        <config name=""RemoteIpAddress"" value=""127.0.0.1"" type=""String""/>
                                    </config>
	                            </config>
                            </root>";

            using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml))
            {
                Assert.Equal("127.0.0.1", source.GetValue<string>("SetUp.Address.RemoteIpAddress"));
                Assert.NotEqual("127.0.0.0", source.GetValue<string>("SetUp.Address.RemoteIpAddress"));
                source.SetValue("SetUp.Address.RemoteIpAddress", "127.0.0.0");
                Assert.Equal("127.0.0.0", source.GetValue<string>("SetUp.Address.RemoteIpAddress"));
            }
        }

        [Fact]
        public void SetValue_Test()
        {
            using (PrimitiveConfigSource source = new PrimitiveConfigSource("system.xml", Encoding.UTF8))
            {
                // Boolean
                source.SetValue("System.IsATMMode", true);
                source.SetValue("System.IsATMMode", false);
                source.SetValue("System.IsATMMode", "true");
                source.SetValue("System.IsATMMode", "false");
                source.SetValue("System.IsATMMode", "TRUe");
                source.SetValue("System.IsATMMode", "faLSe");
                // Decimal
                source.SetValue("System.Scheduler.WaitLoadTimeOut", 12);
                source.SetValue("System.Scheduler.WaitLoadTimeOut", "12");
                source.SetValue("System.Scheduler.WaitLoadTimeOut", 0XA0);
                source.SetValue("System.Scheduler.WaitLoadTimeOut", "0xAB");
                source.SetValue("System.Scheduler.WaitLoadTimeOut", "0XB");
                source.SetValue("System.Scheduler.WaitLoadTimeOut", 12.0);
                source.SetValue("System.Scheduler.WaitLoadTimeOut", -12.12);
                source.SetValue("System.Scheduler.WaitLoadTimeOut", "1,23.09");
                source.SetValue("System.Scheduler.WaitLoadTimeOut", -1.1e2);
                source.SetValue("System.Scheduler.WaitLoadTimeOut", "-1.1e2");
                source.SetValue("System.Scheduler.WaitLoadTimeOut", "1,111.1e2");
                // Integer
                source.SetValue("System.CycleCount", 1);
                source.SetValue("System.CycleCount", 1.0);
                source.SetValue("System.CycleCount", "-1.0");
                source.SetValue("System.CycleCount", "1");
                source.SetValue("System.CycleCount", "0xA");
                source.SetValue("System.CycleCount", "0XA01");
                source.SetValue("System.CycleCount", "12,34");
                source.SetValue("System.CycleCount", "12,34.0");
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.CycleCount", 1.1));
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.CycleCount", "1.01"));
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.CycleCount", "1e2"));
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.CycleCount", "0.111e2"));
                // String
                source.SetValue("System.MonitoredDriveLetter", "");
                source.SetValue("System.MonitoredDriveLetter", 1);
                source.SetValue("System.MonitoredDriveLetter", false);
                source.SetValue("System.MonitoredDriveLetter", 1.1);
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.MonitoredDriveLetter", null));
                // Folder
                source.SetValue("System.LogsFolder", "D:\\");
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.LogsFolder", "D:"));
                // File
                source.SetValue("System.DataReport", "D:\\data.xlsx");
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.DataReport", "D:\\data.xlsx\\"));
                // DateTime
                source.SetValue("System.ResetDate", DateTime.Now);
                source.SetValue("System.ResetDate", "2025-8-4");
                // Color
                source.SetValue("System.AlarmLight", "#000000CC");
                source.SetValue("System.AlarmLight", "#0000CC");
                source.SetValue("System.AlarmLight", System.Drawing.Color.MediumBlue);
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.AlarmLight", "FFFFFF"));
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.AlarmLight", "FFFFFFFF"));
                // Multi Sets
                source.SetValue(("System.IsATMMode", true), ("System.CycleCount", 1));
                Assert.True(source.GetValue<bool>("System.IsATMMode"));
                Assert.Equal(1, source.GetValue<int>("System.CycleCount"));
            }
        }

        [Fact]
        public void ValidateByMaxMin_Test()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <root>
	                            <config name=""System"">
		                            <config name=""Count1"" value=""10"" type=""Integer"" />
		                            <config name=""Count2"" value=""10"" type=""Integer"" max=""""/>
                                    <config name=""Count3"" value=""-1,000"" type=""Integer"" />
                                    <config name=""Count4"" value=""10,00,00"" type=""Integer"" />
                                    <config name=""Count5"" value=""1.00"" type=""Integer"" />
                                    <config name=""Count6"" value=""12,345.00"" type=""Integer"" />

                                    <config name=""Time1"" value=""6.5"" type=""Decimal"" />
                                    <config name=""Time2"" value=""6"" type=""Decimal"" />
                                    <config name=""Time3"" value=""-6.5"" type=""Decimal"" />
                                    <config name=""Time4"" value=""1,234.5"" type=""Decimal"" />
                                    <config name=""Time5"" value=""0x0A"" type=""Decimal"" />
                                    <config name=""Time6"" value=""1,234.5e-3"" type=""Decimal"" />
	                            </config>
                            </root>";

            using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml)) { }
        }

        [Fact]
        public void ValidateByOptions_Test()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <root>
	                        <config name=""System"">
		                        <config name=""CycleCount"" value=""1"" type=""Integer"" options=""1;02;0x03""/>
                                <config name=""DelayTime"" value=""03"" type=""Decimal"" options=""1;02;0x03;11,33e2;-3.14""/>
	                        </config>
                            <config name=""SetUp"">
                                <config name=""Address"">
                                    <config name=""RemoteIpAddress"" value=""127.0.0.3"" type=""String"" options=""127.0.0.2;127.0.0.3;127.0.0.4""/>
                                </config>
	                        </config>
                        </root>";

            using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml))
            {
                source.SetValue("System.CycleCount", 1);
                source.SetValue("System.CycleCount", 2);
                source.SetValue("System.CycleCount", 3);
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.CycleCount", 5));

                source.SetValue("System.DelayTime", 1);
                source.SetValue("System.DelayTime", 2);
                source.SetValue("System.DelayTime", -3.140);
                source.SetValue("System.DelayTime", 3);
                source.SetValue("System.DelayTime", "0x03");
                source.SetValue("System.DelayTime", "0x00003");
                source.SetValue("System.DelayTime", 113300);

                Assert.ThrowsAny<Exception>(() => source.SetValue("System.DelayTime", 1133000));
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.DelayTime", -3.141));
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.DelayTime", "0x00004"));

                source.SetValue("SetUp.Address.RemoteIpAddress", "127.0.0.3");
                Assert.ThrowsAny<Exception>(() => source.SetValue("SetUp.Address.RemoteIpAddress", "127.0.0.1"));
            }
        }

        [Fact]
        public void ValidateByRegex_Test()
        {
            // Use regular expressions to restrict System.Count to only be an even number between -10 and 20
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <root>
	                        <config name=""System"">
		                        <config name=""Count"" value=""0X06"" type=""Integer"" regex=""^(?:-10|-(?:[2468])|0|(?:[2468])|1(?:[02468])|20)$""/>
                                <config name=""Email"" value=""xuyue@qq.com"" type=""string"" regex=""^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"" />
	                        </config>
                        </root>";

            using (PrimitiveConfigSource source = new PrimitiveConfigSource(xml))
            {
                source.SetValue("System.Count", -8);
                source.SetValue("System.Count", 0);
                source.SetValue("System.Count", 18);
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.Count", 26));
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.Count", -12));
                source.SetValue("System.Email", "li.liuwei@outlook.com");
                Assert.ThrowsAny<Exception>(() => source.SetValue("System.Email", "li.liuwei@outlookcom"));
            }
        }
    }
}