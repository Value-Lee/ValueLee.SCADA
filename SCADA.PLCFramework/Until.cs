using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SCADA.PLCFramework
{
    internal static class Until
    {
        public static int GetByteCount(ValueType valueType)
        {
            switch (valueType)
            {
                case ValueType.@bool:
                    return 1;

                case ValueType.int8:
                    return 1;

                case ValueType.int16:
                    return 2;

                case ValueType.int32:
                    return 4;

                case ValueType.int64:
                    return 8;

                case ValueType.uint8:
                    return 1;

                case ValueType.uint16:
                    return 2;

                case ValueType.uint32:
                    return 4;

                case ValueType.uint64:
                    return 8;

                case ValueType.@float:
                    return 4;

                case ValueType.@double:
                    return 8;
            }
            var valueType2 = valueType.ToString();
            if (valueType2.StartsWith("string"))
            {
                return int.Parse(valueType2.Replace("string", ""));
            }

            throw new ApplicationException();
        }

        public static void ValidatePlcInfoContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("PLC info content cannot be null or empty.");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(content);
            // 根节点必须是PlcInfo
            var plcinfoNode = xmlDoc.SelectSingleNode("/PlcInfo") ?? throw new FormatException("Invalid PLC info content: Missing root 'PlcInfo' element.");
            // 节点只能是 Block DIs DOs AIs AOs Item
            plcinfoNode.ChildNodes.Cast<XmlNode>().ToList().ForEach(node =>
            {
                if (node.Name != "DIs" && node.Name != "DOs" && node.Name != "AIs" && node.Name != "AOs" && node.Name != "Block")
                    throw new FormatException($"Invalid PLC info content: Unexpected element '{node.Name}' in 'PlcInfo' element.Hints:{node.OuterXml}.");
            });
            var disNode = plcinfoNode.SelectSingleNode("DIs");
            disNode?.ChildNodes.Cast<XmlNode>().ToList().ForEach(node =>
                {
                    if (node.Name != "Item")
                        throw new FormatException($"Invalid PLC info content: Unexpected element '{node.Name}' in 'DIs' element.Hints:{node.OuterXml}.");
                });
            var dosNode = plcinfoNode.SelectSingleNode("DOs");
            dosNode?.ChildNodes.Cast<XmlNode>().ToList().ForEach(node =>
                {
                    if (node.Name != "Item")
                        throw new FormatException($"Invalid PLC info content: Unexpected element '{node.Name}' in 'DOs' element.Hints:{node.OuterXml}");
                });
            var aisNode = plcinfoNode.SelectSingleNode("AIs");
            aisNode?.ChildNodes.Cast<XmlNode>().ToList().ForEach(node =>
                {
                    if (node.Name != "Item")
                        throw new FormatException($"Invalid PLC info content: Unexpected element '{node.Name}' in 'AIs' element.Hints:{node.OuterXml}");
                });
            var aosNode = plcinfoNode.SelectSingleNode("AOs");
            aosNode?.ChildNodes.Cast<XmlNode>().ToList().ForEach(node =>
                {
                    if (node.Name != "Item")
                        throw new FormatException($"Invalid PLC info content: Unexpected element '{node.Name}' in 'AOs' element.Hints:{node.OuterXml}");
                });
            // 节点Block可以出现多次，但至少出现一次
            if (plcinfoNode.SelectNodes("Block").Count < 1)
                throw new FormatException("Invalid PLC info content: 'Block' element must appear at least once in 'PlcInfo' element.");

            // 节点DIs DOs AIs AOs最多只能出现一次
            if (plcinfoNode.SelectNodes("DIs").Count > 1)
                throw new FormatException("Invalid PLC info content: 'DIs' element can only appear once in 'PlcInfo' element.");
            if (plcinfoNode.SelectNodes("DOs").Count > 1)
                throw new FormatException("Invalid PLC info content: 'DOs' element can only appear once in 'PlcInfo' element.");
            if (plcinfoNode.SelectNodes("AIs").Count > 1)
                throw new FormatException("Invalid PLC info content: 'AIs' element can only appear once in 'PlcInfo' element.");
            if (plcinfoNode.SelectNodes("AOs").Count > 1)
                throw new FormatException("Invalid PLC info content: 'AOs' element can only appear once in 'PlcInfo' element.");

            #region PlcInfo

            var plcinfoAttrs = plcinfoNode.Attributes.Cast<XmlAttribute>().ToList();
            plcinfoAttrs.ForEach(attr =>
            {
                if (attr.Name != "ip" && attr.Name != "port" && attr.Name != "class" && attr.Name != "assembly")
                    throw new FormatException($"Invalid PLC info content: Unexpected attribute '{attr.Name}' in 'PlcInfo' element.Hits:{plcinfoNode.OuterXml}.");
            });
            var ip = plcinfoNode.Attributes["ip"];
            if (ip == null)
                throw new FormatException($"Invalid PLC info content: Missing required 'ip' attribute in 'PlcInfo' element.Hits:{plcinfoNode.OuterXml}.");
            if (IPAddress.TryParse(ip.Value.Trim(), out var _) == false)
                throw new FormatException($"Invalid PLC info content: 'ip' attribute must be a valid IP address.Hint:'{plcinfoNode.OuterXml}'.");
            var port = plcinfoNode.Attributes["port"];
            if (port == null)
                throw new FormatException($"Invalid PLC info content: Missing required 'port' attribute in 'PlcInfo' element.Hints:'{plcinfoNode.OuterXml}'.");
            if (!int.TryParse(port.Value.Trim(), out int portNum) || portNum <= 0 || portNum > 65535)
                throw new FormatException($"Invalid PLC info content: 'port' attribute must be a valid integer between 1 and 65535.Hints:'{plcinfoNode.OuterXml}'.");

            var assembly = plcinfoNode.Attributes["assembly"];
            if (assembly == null)
                throw new FormatException($"Invalid PLC info content: Missing required 'assembly' attribute in 'PlcInfo' element.Hints:'{plcinfoNode.OuterXml}'.");
            try
            {
                Assembly.Load(assembly.Value.Trim());
            }
            catch (FileNotFoundException)
            {
                throw new FormatException($"Invalid PLC info content: Not found assembly '{assembly.Value}'.Hints:'{plcinfoNode.OuterXml}'.");
            }
            catch (Exception)
            {
                throw new FormatException($"Invalid PLC info content: Cannot load assembly '{assembly.Value}'.Hints:'{plcinfoNode.OuterXml}'.");
            }
            var classAttr = plcinfoNode.Attributes["class"];
            if (classAttr == null)
                throw new FormatException($"Invalid PLC info content: Missing required 'class' attribute in 'PlcInfo' element.Hints:'{plcinfoNode.OuterXml}'.");
            var _ = Assembly.Load(assembly.Value.Trim()).GetType(classAttr.Value.Trim()) ?? throw new FormatException($"Invalid PLC info content: Cannot find class '{classAttr.Value}' in assembly '{assembly.Value}'.Hints:'{plcinfoNode.OuterXml}'.");

            #endregion PlcInfo

            #region Block

            var blockNodes = plcinfoNode.SelectNodes("Block").Cast<XmlNode>().ToList();
            if (blockNodes != null)
            {
                var ids = new List<string>();
                foreach (var blockNode in blockNodes)
                {
                    // 不能含有无关属性
                    blockNode.Attributes.Cast<XmlAttribute>().ToList().ForEach(attr =>
                    {
                        if (attr.Name != "id" && attr.Name != "type" && attr.Name != "startaddr" && attr.Name != "len" && attr.Name != "polling")
                            throw new FormatException($"Invalid PLC info content: Unexpected attribute '{attr.Name}' in 'Block' element.Hints:{blockNode.OuterXml}.");
                    });
                    /* Block节点属性值合法性检查*/

                    var id = blockNode.Attributes["id"];
                    if (id == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'id' attribute in 'Block' element.Hints:{blockNode.OuterXml}.");
                    if (string.IsNullOrWhiteSpace(id.Value))
                        throw new FormatException($"Invalid PLC info content: 'id' attribute in 'Block' element cannot be null or empty.Hints:{blockNode.OuterXml}.");
                    // id不能重复
                    if (ids.Contains(id.Value.Trim()))
                    {
                        throw new FormatException($"Invalid PLC info content: Duplicate 'id' attribute value '{id.Value}' in 'Block' elements.Hints:{blockNode.OuterXml}.");
                    }
                    ids.Add(id.Value.Trim());

                    var type = blockNode.Attributes["type"];
                    if (type == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'type' attribute in 'Block' element.Hints:{blockNode.OuterXml}.");
                    // type必须是di/do/ai/ao之一
                    if (type.Value.Trim() != "di" && type.Value.Trim() != "do" && type.Value.Trim() != "ai" && type.Value.Trim() != "ao")
                        throw new FormatException($"Invalid PLC info content: 'type' attribute in 'Block' element must be one of the following values: di, do, ai, ao.Hints:{blockNode.OuterXml}.");

                    var startaddr = blockNode.Attributes["startaddr"];
                    if (startaddr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'startaddr' attribute in 'Block' element.Hints:{blockNode.OuterXml}.");
                    if (string.IsNullOrWhiteSpace(startaddr.Value))
                        throw new FormatException($"Invalid PLC info content: 'startaddr' attribute in 'Block' element cannot be null or empty.Hints:{blockNode.OuterXml}.");

                    var len = blockNode.Attributes["len"];
                    if (len == null || string.IsNullOrWhiteSpace(len.Value.Trim()) || !int.TryParse(len.Value, out int lenNum) || lenNum <= 0)
                        throw new FormatException("Invalid PLC info content: 'len' attribute in 'Block' element must be a valid positive integer.");

                    var polling = blockNode.Attributes["polling"];
                    if (polling == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'polling' attribute in 'Block' element.Hints:{blockNode.OuterXml}.");
                    if (bool.TryParse(polling.Value, out var _) == false)
                        throw new FormatException($"Invalid PLC info content: 'polling' attribute in 'Block' element must be a valid boolean value (true or false).Hints:{blockNode.OuterXml}.");
                }
            }

            #endregion Block

            var names = new List<string>();

            #region DIs

            if (disNode != null)
            {
                var itemNodes = disNode.ChildNodes.Cast<XmlNode>().ToList();

                foreach (var itemNode in itemNodes)
                {
                    var itemAttrs = itemNode.Attributes.Cast<XmlAttribute>().ToList();
                    // 不能含有无关属性
                    itemAttrs.ForEach(attr =>
                    {
                        if (attr.Name != "name" && attr.Name != "addr" && attr.Name != "type" && attr.Name != "desc" && attr.Name != "display" && attr.Name != "blockid")
                            throw new FormatException($"Invalid PLC info content: Unexpected attribute '{attr.Name}' in 'Item' element under 'DIs',attribute name must be name,addr,type,blockid,display or desc. Hints:{itemNode.OuterXml}.");
                    });

                    /* Item节点属性值合法性检查*/

                    var nameAttr = itemNode.Attributes["name"];
                    if (nameAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'name' attribute in 'Item' element under 'DIs'. Hints:{itemNode.OuterXml}.");
                    // name必须以DI_开头
                    if (!nameAttr.Value.Trim().StartsWith("DI_"))
                    {
                        throw new FormatException($"Invalid PLC info content: 'name' attribute in 'Item' element under 'DIs' must start with 'DI_'. Hints:{itemNode.OuterXml}.");
                    }
                    // name不能重复
                    if (names.Contains(nameAttr.Value.Trim()))
                    {
                        throw new FormatException($"Invalid PLC info content: Duplicate 'name' attribute value '{nameAttr.Value}' in 'Item' elements under 'DIs'. Hints:{itemNode.OuterXml}.");
                    }
                    names.Add(nameAttr.Value.Trim());

                    var blockidAttr = itemNode.Attributes["blockid"];
                    if (blockidAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'blockid' attribute in 'Item' element under 'DIs'. Hints:{itemNode.OuterXml}.");
                    if (string.IsNullOrWhiteSpace(blockidAttr.Value))
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'DIs' cannot be null or empty. Hints:{itemNode.OuterXml}.");
                    // 所属的Block必须存在
                    if (blockNodes.Select(x => x.Attributes["id"].Value.Trim()).ToList().Contains(blockidAttr.Value.Trim()) == false)
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'DIs' must refer to an existing Block id. Hints:{itemNode.OuterXml}.");
                    // 所属的Block类型必须是di
                    var refBlockNode = blockNodes.First(x => x.Attributes["id"].Value.Trim() == blockidAttr.Value.Trim());
                    if (refBlockNode.Attributes["type"].Value.Trim() != "di")
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'DIs' must refer to a Block with type 'di'. Hints:{itemNode.OuterXml}.");

                    var addrAttr = itemNode.Attributes["addr"];
                    if (addrAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'addr' attribute in 'Item' element under 'DIs'. Hints:{itemNode.OuterXml}.");
                    // 必须是正整数，且不大于所属Block的len属性值
                    if (int.TryParse(addrAttr.Value, out int addrAttrNum) == false)
                        throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'DIs' must be a valid integer. Hints:{itemNode.OuterXml}.");
                    if (addrAttrNum < 0)
                        throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'DIs' must be a non-negative integer. Hints:{itemNode.OuterXml}.");
                    var blockNode = blockNodes.First(x => x.Attributes["id"].Value.Trim() == blockidAttr.Value.Trim());
                    if (addrAttrNum > int.Parse(blockNode.Attributes["len"].Value.Trim()))
                        throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'DIs' must not be more than the 'len' attribute of the referenced Block. Hints:{itemNode.OuterXml}.");

                    var typeAttr = itemNode.Attributes["type"];
                    if (typeAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'type' attribute in 'Item' element under 'DIs'. Hints:{itemNode.OuterXml}.");
                    // 必须是bool类型
                    if (!Enum.TryParse(typeAttr.Value.Trim(), out ValueType type) || type != ValueType.@bool)
                        throw new FormatException($"Invalid PLC info content: 'type' attribute in 'Item' element under 'DIs' must be 'bool'. Hints:{itemNode.OuterXml}.");

                    var displayAttr = itemNode.Attributes["display"];
                    if (displayAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'display' attribute in 'Item' element under 'DIs'. Hints:{itemNode.OuterXml}.");

                    var descAttr = itemNode.Attributes["desc"];
                    if (descAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'desc' attribute in 'Item' element under 'DIs'. Hints:{itemNode.OuterXml}.");
                }
            }

            #endregion DIs

            #region DOs

            if (dosNode != null)
            {
                var itemNodes = dosNode.ChildNodes.Cast<XmlNode>().ToList();

                foreach (var itemNode in itemNodes)
                {
                    var itemAttrs = itemNode.Attributes.Cast<XmlAttribute>().ToList();
                    // 不能含有无关属性
                    itemAttrs.ForEach(attr =>
                    {
                        if (attr.Name != "name" && attr.Name != "addr" && attr.Name != "type" && attr.Name != "desc" && attr.Name != "display" && attr.Name != "blockid")
                            throw new FormatException($"Invalid PLC info content: Unexpected attribute '{attr.Name}' in 'Item' element under 'DOs',attribute name must be name,addr,type,blockid,display or desc. Hints:{itemNode.OuterXml}.");
                    });

                    /* Item节点属性值合法性检查*/

                    var nameAttr = itemNode.Attributes["name"];
                    if (nameAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'name' attribute in 'Item' element under 'DOs'. Hints:{itemNode.OuterXml}.");
                    // name必须以DO_开头
                    if (!nameAttr.Value.Trim().StartsWith("DO_"))
                    {
                        throw new FormatException($"Invalid PLC info content: 'name' attribute in 'Item' element under 'DOs' must start with 'DO_'. Hints:{itemNode.OuterXml}.");
                    }
                    // name不能重复
                    if (names.Contains(nameAttr.Value.Trim()))
                    {
                        throw new FormatException($"Invalid PLC info content: Duplicate 'name' attribute value '{nameAttr.Value}' in 'Item' elements under 'DOs'. Hints:{itemNode.OuterXml}.");
                    }
                    names.Add(nameAttr.Value.Trim());

                    var blockidAttr = itemNode.Attributes["blockid"];
                    if (blockidAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'blockid' attribute in 'Item' element under 'DOs'. Hints:{itemNode.OuterXml}.");
                    if (string.IsNullOrWhiteSpace(blockidAttr.Value))
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'DOs' cannot be null or empty. Hints:{itemNode.OuterXml}.");
                    // 所属的Block必须存在
                    if (blockNodes.Select(x => x.Attributes["id"].Value.Trim()).ToList().Contains(blockidAttr.Value.Trim()) == false)
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'DOs' must refer to an existing Block id. Hints:{itemNode.OuterXml}.");
                    // 所属的Block类型必须是do
                    var refBlockNode = blockNodes.First(x => x.Attributes["id"].Value.Trim() == blockidAttr.Value.Trim());
                    if (refBlockNode.Attributes["type"].Value.Trim() != "do")
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'DOs' must refer to a Block with type 'do'. Hints:{itemNode.OuterXml}.");

                    var addrAttr = itemNode.Attributes["addr"];
                    if (addrAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'addr' attribute in 'Item' element under 'DOs'. Hints:{itemNode.OuterXml}.");
                    // 必须是正整数，且不大于所属Block的len属性值
                    if (int.TryParse(addrAttr.Value, out int addrAttrNum) == false)
                        throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'DOs' must be a valid integer. Hints:{itemNode.OuterXml}.");
                    if (addrAttrNum < 0)
                        throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'DOs' must be a non-negative integer. Hints:{itemNode.OuterXml}.");
                    var blockNode = blockNodes.First(x => x.Attributes["id"].Value.Trim() == blockidAttr.Value.Trim());
                    if (addrAttrNum > int.Parse(blockNode.Attributes["len"].Value.Trim()))
                        throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'DOs' must not be more than the 'len' attribute of the referenced Block. Hints:{itemNode.OuterXml}.");

                    var typeAttr = itemNode.Attributes["type"];
                    if (typeAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'type' attribute in 'Item' element under 'DOs'. Hints:{itemNode.OuterXml}.");
                    // 必须是bool类型
                    if (!Enum.TryParse(typeAttr.Value.Trim(), out ValueType type) || type != ValueType.@bool)
                        throw new FormatException($"Invalid PLC info content: 'type' attribute in 'Item' element under 'DOs' must be 'bool'. Hints:{itemNode.OuterXml}.");

                    var displayAttr = itemNode.Attributes["display"];
                    if (displayAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'display' attribute in 'Item' element under 'DOs'. Hints:{itemNode.OuterXml}.");

                    var descAttr = itemNode.Attributes["desc"];
                    if (descAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'desc' attribute in 'Item' element under 'DOs'. Hints:{itemNode.OuterXml}.");
                }
            }

            #endregion DOs

            #region AIs

            if (aisNode != null)
            {
                // 非bool地址必须是整数,地址加长度必须小于总长
                // bool地址必须是1.0-1.7形式，或整数形式
                // string类型的长度必须小于总长

                var itemNodes = aisNode.ChildNodes.Cast<XmlNode>().ToList();

                foreach (var itemNode in itemNodes)
                {
                    var itemAttrs = itemNode.Attributes.Cast<XmlAttribute>().ToList();
                    // 不能含有无关属性
                    itemAttrs.ForEach(attr =>
                    {
                        if (attr.Name != "name" && attr.Name != "addr" && attr.Name != "type" && attr.Name != "desc" && attr.Name != "display" && attr.Name != "blockid")
                            throw new FormatException($"Invalid PLC info content: Unexpected attribute '{attr.Name}' in 'Item' element under 'AIs',attribute name must be name,addr,type,blockid,display or desc. Hints:{itemNode.OuterXml}.");
                    });

                    /* Item节点属性值合法性检查*/

                    var nameAttr = itemNode.Attributes["name"];
                    if (nameAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'name' attribute in 'Item' element under 'AIs'. Hints:{itemNode.OuterXml}.");
                    // name必须以AI_开头
                    if (!nameAttr.Value.Trim().StartsWith("AI_"))
                    {
                        throw new FormatException($"Invalid PLC info content: 'name' attribute in 'Item' element under 'AIs' must start with 'AI_'. Hints:{itemNode.OuterXml}.");
                    }
                    // name不能重复
                    if (names.Contains(nameAttr.Value.Trim()))
                    {
                        throw new FormatException($"Invalid PLC info content: Duplicate 'name' attribute value '{nameAttr.Value}' in 'Item' elements under 'AIs'. Hints:{itemNode.OuterXml}.");
                    }
                    names.Add(nameAttr.Value.Trim());

                    var blockidAttr = itemNode.Attributes["blockid"];
                    if (blockidAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'blockid' attribute in 'Item' element under 'AIs'. Hints:{itemNode.OuterXml}.");
                    if (string.IsNullOrWhiteSpace(blockidAttr.Value))
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'AIs' cannot be null or empty. Hints:{itemNode.OuterXml}.");
                    // 所属的Block必须存在
                    if (blockNodes.Select(x => x.Attributes["id"].Value.Trim()).ToList().Contains(blockidAttr.Value.Trim()) == false)
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'AIs' must refer to an existing Block id. Hints:{itemNode.OuterXml}.");
                    // 所属的Block类型必须是AI
                    var refBlockNode = blockNodes.First(x => x.Attributes["id"].Value.Trim() == blockidAttr.Value.Trim());
                    if (refBlockNode.Attributes["type"].Value.Trim() != "ai")
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'AIs' must refer to a Block with type 'ai'. Hints:{itemNode.OuterXml}.");

                    var typeAttr = itemNode.Attributes["type"];
                    if (typeAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'type' attribute in 'Item' element under 'AIs'. Hints:{itemNode.OuterXml}.");
                    // 必须是ValueType类型
                    if (!Enum.TryParse(typeAttr.Value.Trim(), out ValueType type))
                        throw new FormatException($"Invalid PLC info content: 'type' attribute in 'Item' element under 'DOs' must be one of {string.Join(",", Enum.GetNames(typeof(ValueType)))}. Hints:{itemNode.OuterXml}.");

                    var addrAttr = itemNode.Attributes["addr"];
                    if (addrAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'addr' attribute in 'Item' element under 'AIs'. Hints:{itemNode.OuterXml}.");

                    // 如果不是bool类型，那么必须是正整数，且不大于所属Block的len属性值
                    if (type != ValueType.@bool)
                    {
                        if (int.TryParse(addrAttr.Value, out int addrAttrNum) == false)
                            throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'AIs' must be a valid integer if type isn {type}. Hints:{itemNode.OuterXml}.");
                        if (addrAttrNum < 0)
                            throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'AIs' must be a non-negative integer if type is {type}. Hints:{itemNode.OuterXml}.");
                        var blockNode = blockNodes.First(x => x.Attributes["id"].Value.Trim() == blockidAttr.Value.Trim());
                        if (GetByteCount(type) + addrAttrNum + 1 > int.Parse(blockNode.Attributes["len"].Value.Trim()))
                        {
                            throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'AIs' plus {type} length can't more than the 'len' attribute of the referenced Block.Hints:{itemNode.OuterXml}.");
                        }
                    }
                    else
                    {
                        if (!Regex.Match(addrAttr.Value.Trim(), "^[1-9][0-9]*\\.[0-7]$").Success)
                        {
                            throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'AIs' must be like '1.0,1.1,1.2...1.6,1.7, 2.1,2.2...2.6,2.7, 3.1,3.2...3.7...' if type is '{type}'. Hints:{itemNode.OuterXml}.");
                        }
                        var blockNode = blockNodes.First(x => x.Attributes["id"].Value.Trim() == blockidAttr.Value.Trim());
                        if ((int)float.Parse(addrAttr.Value.Trim()) + GetByteCount(type) + 1 > int.Parse(blockNode.Attributes["len"].Value.Trim()))
                        {
                            throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'AIs' plus {type} length must not be more than the 'len' attribute of the referenced Block.Hints:{itemNode.OuterXml}.");
                        }
                    }

                    var displayAttr = itemNode.Attributes["display"];
                    if (displayAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'display' attribute in 'Item' element under 'AIs'. Hints:{itemNode.OuterXml}.");

                    var descAttr = itemNode.Attributes["desc"];
                    if (descAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'desc' attribute in 'Item' element under 'AIs'. Hints:{itemNode.OuterXml}.");
                }
            }

            #endregion AIs

            #region AOs

            if (aosNode != null)
            {
                var itemNodes = aosNode.ChildNodes.Cast<XmlNode>().ToList();

                foreach (var itemNode in itemNodes)
                {
                    var itemAttrs = itemNode.Attributes.Cast<XmlAttribute>().ToList();
                    // 不能含有无关属性
                    itemAttrs.ForEach(attr =>
                    {
                        if (attr.Name != "name" && attr.Name != "addr" && attr.Name != "type" && attr.Name != "desc" && attr.Name != "display" && attr.Name != "blockid")
                            throw new FormatException($"Invalid PLC info content: Unexpected attribute '{attr.Name}' in 'Item' element under 'AOs',attribute name must be one of name,addr,type,blockid,display or desc. Hints:{itemNode.OuterXml}.");
                    });

                    /* Item节点属性值合法性检查*/
                    var nameAttr = itemNode.Attributes["name"];
                    if (nameAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'name' attribute in 'Item' element under 'AOs'. Hints:{itemNode.OuterXml}.");
                    // name必须以AO_开头
                    if (!nameAttr.Value.Trim().StartsWith("AO_"))
                        throw new FormatException($"Invalid PLC info content: 'name' attribute in 'Item' element under 'AOs' must start with 'AO_'. Hints:{itemNode.OuterXml}.");
                    // name不能重复
                    if (names.Contains(nameAttr.Value.Trim()))
                        throw new FormatException($"Invalid PLC info content: Duplicate 'name' attribute value '{nameAttr.Value}' in 'Item' elements under 'AOs'. Hints:{itemNode.OuterXml}.");
                    names.Add(nameAttr.Value.Trim());

                    var blockidAttr = itemNode.Attributes["blockid"];
                    if (blockidAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'blockid' attribute in 'Item' element under 'AOs'. Hints:{itemNode.OuterXml}.");
                    if (string.IsNullOrWhiteSpace(blockidAttr.Value))
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'AOs' cann't be null or empty. Hints:{itemNode.OuterXml}.");
                    // 所属的Block必须存在
                    if (blockNodes.Select(x => x.Attributes["id"].Value.Trim()).ToList().Contains(blockidAttr.Value.Trim()) == false)
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'AOs' must refer to an existing Block id. Hints:{itemNode.OuterXml}.");
                    // 所属的Block类型必须是AO
                    var refBlockNode = blockNodes.First(x => x.Attributes["id"].Value.Trim() == blockidAttr.Value.Trim());
                    if (refBlockNode.Attributes["type"].Value.Trim() != "ao")
                        throw new FormatException($"Invalid PLC info content: 'blockid' attribute in 'Item' element under 'AOs' must refer to a Block with type 'ai'. Hints:{itemNode.OuterXml}.");

                    var typeAttr = itemNode.Attributes["type"];
                    if (typeAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'type' attribute in 'Item' element under 'AOs'. Hints:{itemNode.OuterXml}.");
                    // 必须是ValueType类型
                    if (!Enum.TryParse(typeAttr.Value.Trim(), out ValueType type))
                        throw new FormatException($"Invalid PLC info content: 'type' attribute in 'Item' element under 'DOs' must be one of '{string.Join(",", Enum.GetNames(typeof(ValueType)))}'. Hints:{itemNode.OuterXml}.");

                    var addrAttr = itemNode.Attributes["addr"];
                    if (addrAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'addr' attribute in 'Item' element under 'AOs'. Hints:{itemNode.OuterXml}.");

                    // 如果不是bool类型，那么必须是正整数，且不大于所属Block的len属性值
                    if (type != ValueType.@bool)
                    {
                        if (int.TryParse(addrAttr.Value, out int addrAttrNum) == false)
                            throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'AOs' must be a valid integer if type isn {type}. Hints:{itemNode.OuterXml}.");
                        if (addrAttrNum < 0)
                            throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'AOs' must be a non-negative integer if type is {type}. Hints:{itemNode.OuterXml}.");
                        var blockNode = blockNodes.First(x => x.Attributes["id"].Value.Trim() == blockidAttr.Value.Trim());
                        if (GetByteCount(type) + addrAttrNum + 1 > int.Parse(blockNode.Attributes["len"].Value.Trim()))
                        {
                            throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'AOs' plus {type} length can't be more than the 'len' attribute of the referenced Block.Hints:{itemNode.OuterXml}.");
                        }
                    }
                    else
                    {
                        if (!Regex.Match(addrAttr.Value.Trim(), "^[1-9][0-9]*\\.[0-7]$").Success)
                        {
                            throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'AOs' must be like '1.0,1.1,1.2...1.6,1.7, 2.1,2.2...2.6,2.7, 3.1,3.2...3.7...' if type is '{type}'. Hints:{itemNode.OuterXml}.");
                        }
                        var blockNode = blockNodes.First(x => x.Attributes["id"].Value.Trim() == blockidAttr.Value.Trim());
                        if ((int)float.Parse(addrAttr.Value.Trim()) + GetByteCount(type) + 1 > int.Parse(blockNode.Attributes["len"].Value.Trim()))
                        {
                            throw new FormatException($"Invalid PLC info content: 'addr' attribute in 'Item' element under 'AOs' plus {type} length must not be more than the 'len' attribute of the referenced Block.Hints:{itemNode.OuterXml}.");
                        }
                    }

                    var displayAttr = itemNode.Attributes["display"];
                    if (displayAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'display' attribute in 'Item' element under 'AOs'. Hints:{itemNode.OuterXml}.");

                    var descAttr = itemNode.Attributes["desc"];
                    if (descAttr == null)
                        throw new FormatException($"Invalid PLC info content: Missing required 'desc' attribute in 'Item' element under 'AOs'. Hints:{itemNode.OuterXml}.");
                }
            }

            #endregion AOs
        }
    }
}