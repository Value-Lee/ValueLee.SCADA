## Motivation and Function

**.net framework app.config**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2" />
    </startup>
    <appSettings>
        <add key="CycleCount" value="365" />
        <add key="IsSimulatorMode" value="false" />
        <add key="DiskFreeSpaceAlarmTolerance" value="6.18" />
        <add key="RemoteIpAddress" value="127.0.0.1" />
    </appSettings>
</configuration>
```

**缺点**

- 只支持读配置，不支持修改。.NET Framework 程序**在技术上是支持**修改自身的 `app.config` 文件的，但这通常几乎不推荐，`app.config` 文件被设计为存储相对静态的、随应用程序部署的配置信息，例如数据库连接字符串、服务终结点等，而不是用来存储频繁变化的用户数据或运行时状态。

- 结构简单，容易Key重复。如果配置项数量太多，多达几百甚至上千项，很容易导致Key重复。

  ```xml
  <?xml version="1.0" encoding="utf-8" ?>
  <configuration>
      <startup> 
          <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2" />
      </startup>
      <appSettings>
          <add key="System.CycleCount" value="365" />
          <add key="System.IsSimulatorMode" value="false" />
          <add key="System.Setup.DiskFreeSpaceAlarmTolerance" value="6.18" />
          <add key="System.Setup.RemoteIpAddress" value="127.0.0.1" />
      </appSettings>
  </configuration>
  ```

  以`.`延长Key长，虽然在一定程度上避免了Key重复问题，但是结构不易调整。

**PrimitiveConfigSource XML**

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
	<config name="System" >
		<config name="CycleCount" value="3" type="Integer" />
		<config name="IsSimulatorMode" value="false" type="Boolean" />
		<config name="SetUp" >
			<config name="DiskFreeSpaceAlarmTolerance" value="5" type="Decimal" />
			<config name="RemoteIpAddress" value="127.0.0.1"  type="String" />
		</config>
	</config>
</root>
```

- 不仅支持读配置，也支持修改配置
- 高性能写操作。SetValue只要修改完内存中的值立刻返回即刻生效，后台的生产者消费者线程‘默默’写磁盘(.NetFramework4.6.2和.NET6.0，使用Channel不空占线程,开销几乎忽略不计)
- 树状结构，很容易避免Key重复，且容易调整config的位置和Key的索引路径
- 可以在XML中添加额外的数据类型限定，在程序中自动进行类型转换，能够在程序员使用错误的期望类型读写配置时，抛出异常，很好的防呆
- 可以添加校验规则，如最大值最小值限制，正则表达式校验，限制到可允许的取值集合
- 适合配置的结构、内容或键名在编码时无法预知，或者需要被程序动态处理，尤其是自动化行业上位机，需要同一个软件兼容多种机型的场景，避免维护多个分支和软件版本

## Quick Start

### XML File Example

**system.xml**

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
	<config name="System" >
		<config name="CycleCount" value="3" type="Integer" />
		<config name="IsSimulatorMode" value="false" type="Boolean" />
		<config name="SetUp" >
			<config name="DiskFreeSpaceAlarmTolerance" value="5" type="Decimal" />
			<config name="RemoteIpAddress" value="127.0.0.1"  type="String" />
            <config name="LogsFolder" value="C:\Logs"  type="Folder" />
            <config name="DataReport" value="C:\data.csv"  type="File" />
            <config name="AlarmLight" value="#FFFFFF"  type="Color" />
            <config name="ResetDate" value="2025-05-06 08:00:00"  type="DateTime" />
		</config>
	</config>
</root>
```

### Supported Types

- Boolean
- Integer
- Decimal
- String
- Folder
- File
- Color
- DateTime

> Folder,File,Color,DateTime属于非核心type，算是额外拓展的边缘类型，虽然完全可以用String替代，但是这样做的好处是将来做控件来修改XML配置项的值，Folder可以标记弹出文件夹选择对话框，DateTime可以弹出日期选择器，但如果全是String，只能采用简陋的文本框输入路径，颜色，日期，既麻烦也易输入错误，此外，标记成Folder,File,Color,DateTime，PrimitiveConfigSource内部会对Value字符串的格式进行校验检查，避免流入非法字符串。

`当然，如果你明确将来不会通过可视化界面修改某个XML文件的值，那么完全可以不适用Folder，File，Color，DateTime这四种类型，只用String平替即可。`

### Load XML File

```c#
var source = new PrimitiveConfigSource("system.xml",Encoding.UTF8);
```

PrimitiveConfigSource构造函数传入XML文件路径和文件编码。

> 需要保证一个XML文件同时只能有一个PrimitiveConfigSource对象持有，因为单个对象能保证改配置项值时写操作是线程安全的，但多个对象同时写时不是线程安全的。

### Read config item's value using GetValue

####  value="Boolean"

`<config name="IsSimulatorMode" value="false" type="Boolean" />`

```c#
bool isSimulatorMode = source.GetValue<bool>("System.IsSimulatorMode");
```

Boolean支持的字符串样例

- True
- False
- true
- false
- TRuE
- faLsE

true和false的任意大小写形式都能被正常解析。

#### value="Integer"

`<config name="CycleCount" value="3" type="Integer" />`

①

```c#
int cycleCount = source.GetValue<int>("System.CycleCount");
```
②
```c#
long cycleCount = source.GetValue<long>("System.CycleCount");
```
③
```c#
short cycleCount = source.GetValue<short>("System.CycleCount");
```
④
```c#
byte cycleCount = source.GetValue<byte>("System.CycleCount");
```

⑤

```c#
double cycleCount = source.GetValue<double>("System.CycleCount");
```

⑥

```c#
float cycleCount = source.GetValue<float>("System.CycleCount");
```

⑦

```c#
decimal cycleCount = source.GetValue<decimal>("System.CycleCount");
```

①②③④是最‘清真’的写法，⑤⑥⑦存在3次转换，先转换成long，再转换成decimal，最后再转换成传入的泛型double或float或decimal。

如果value文本值是`255`，那么前4种写法都不会有问题。如果value文本值是`70000`，①②(int，long)无问题，③④(short，byte)会溢出异常。在实际编程时，按需决定使用哪一个宽度的数据类型，一般使用int。

Integer支持的字符串格式样例

- `10`
-  `0x0A`
-  `0XB`
-  `-1,000`
-  `10,00,00`
-  `1.00`
-  `12,345.00`

####  value="Decimal"

`<config name="DiskFreeSpaceAlarmTolerance" value="5" type="Decimal" />`

①

```c#
double diskFreeSpaceAlarmTolerance = source.GetValue<double>("System.SetUp.DiskFreeSpaceAlarmTolerance");
```
②
```c#
float diskFreeSpaceAlarmTolerance = source.GetValue<float>("System.SetUp.DiskFreeSpaceAlarmTolerance");
```
③
```c#
decimal diskFreeSpaceAlarmTolerance = source.GetValue<decimal>("System.SetUp.DiskFreeSpaceAlarmTolerance");
```

①②③能表示的范围都足够大，通常不会溢出，最重要的是精度区别。按照实际的精度需求选择最合适的类型，一般double即可。

> 如果value文本值是3.14159265358979323846，共计21位有效数字，选择GetValue\<decimal>()，不会丢失精度，但使用GetValue\<double>得到的是损失5个有效数字的3.1415926535897936。

Decimal支持的字符串格式样例

- `6`
- `0x0A`
- `0XB`
- `6.5`
- `-6.5`
- `1,234.5`
- `1,234.5e-3`

####  value="String"

`<config name="RemoteIpAddress" value="127.0.0.1"  type="String" />`

```c#
string RemoteIpAddress = source.GetValue<string>("System.SetUp.RemoteIpAddress");
```


#### value="Folder"

`<config name="LogsFolder" value="D:\Logs" type="Folder"/>`

```c#
DirectoryInfo folder = source.GetValue<DirectoryInfo>("System.LogsFolder");
```

#### value="File"

`<config name="DataReport" value="C:\data.csv" type="File"/>`

```c#
FileInfo file = source.GetValue<FileInfo>("System.DataReport");
```

#### value="Color"

`<config name="AlarmLight" value="#FFFFFF"  type="Color" />`

```c#
source.GetValue<System.Drawing.Color>("System.AlarmLight");
```

- #FFFFFF
- #0AFFFFFF

#### value="DateTime"

`<config name="ResetDate" value="2025-05-06 08:00:00"  type="DateTime" />`

```c#
DateTime dateTime = source.GetValue<DateTime>("System.ResetDate");
```

- 2020-02-02 02:02:02
- 2020-02-02
- 02:02:02
- 2020/2/2
- 2020/02/02
- 02:02


> type="Integer"，允许映射的C#数据类型是int，long，short，byte，double，float，decimal。
>
> type="Boolean"，允许映射的C#数据类型是bool。
>
> type="Decimal"，允许映射的C#数据类型是double，float，decimal。
>
> type="string"，允许映射的C#数据类型是string。
>
> type="Folder"，允许映射的C#数据类型是DirectoryInfo。
>
> type="File"，允许映射的C#数据类型是FileInfo。
>
> type="Color"，允许映射的C#数据类型是System.Drawing.Color。
>
> type="DateTime"，允许映射的C#数据类型是DateTime。



> ==使用非允许的数据类型时，GetValue\<TValue>()会抛出异常。GetValue\<string>()支持任意type，它读取的是value在XML中的原始文本。如 `<config value="0x0A" type="Integer" />` ，GetValue\<int>()得到的是`10`，GetValue\<string>得到的是`0x0A`字符串。==



### Modify config item's value using SetValue

#### value="Boolean"

```c#
source.SetValue("System.IsSimulatorMode", true);
```

```c#
source.SetValue("System.IsSimulatorMode", false);
```

```c#
source.SetValue("System.IsSimulatorMode", "fAlsE");
```

```c#
source.SetValue("System.IsSimulatorMode", "TRUE");
```


#### value="Integer"

```c#
source.SetValue("System.CycleCount", 13);
```

```c#
source.SetValue("System.CycleCount", "-13");
```

```c#
source.SetValue("System.CycleCount", 13.0);
```

```c#
source.SetValue("System.CycleCount", "13.0");
```

```c#
source.SetValue("System.CycleCount", 0xA2);
```

```c#
source.SetValue("System.CycleCount", "0xA2");
```

```c#
source.SetValue("System.CycleCount", "0XA01");
```

```c#
source.SetValue("System.CycleCount", 123,456);
```

```c#
source.SetValue("System.CycleCount", 12,34,56);
```

```c#
source.SetValue("System.CycleCount", "123,456");
```

#### value="Decimal"

```c#
source.SetValue("System.SetUp.DiskFreeSpaceAlarmTolerance", -23.01);
```

```c#
source.SetValue("System.SetUp.DiskFreeSpaceAlarmTolerance", -1,234.61);
```

```c#
 source.SetValue("System.SetUp.DiskFreeSpaceAlarmTolerance", "34.55");
```

```c#
source.SetValue("System.SetUp.DiskFreeSpaceAlarmTolerance", -69.8e3);
```

```c#
source.SetValue("System.SetUp.DiskFreeSpaceAlarmTolerance", "-69.8e3");
```

```c#
source.SetValue("System.SetUp.DiskFreeSpaceAlarmTolerance", 23);
```

```c#
source.SetValue("System.SetUp.DiskFreeSpaceAlarmTolerance", "0XA01");
```

```c#
source.SetValue("System.SetUp.DiskFreeSpaceAlarmTolerance", 0xA2);
```

```c#
source.SetValue("System.SetUp.DiskFreeSpaceAlarmTolerance", "0xA2");
```

#### value="String"

可以是任意字符串，包括空字符串，也可以是任意数据类型，比如下面的23，会自动调用其ToString()。只要不是null都可以。

```c#
source.SetValue("System.SetUp.RemoteIpAddress", "");
```

```c#
source.SetValue("System.SetUp.RemoteIpAddress", "hello");
```

```c#
source.SetValue("System.SetUp.RemoteIpAddress", 23);
```

#### value="DateTime"

```c#
source.SetValue("System.ResetDate", DateTime.Now);
source.SetValue("System.ResetDate", "2025-8-4");
```

#### value="Color"

```c#
source.SetValue("System.AlarmLight", "#000000CC");
source.SetValue("System.AlarmLight", "#0000CC");
source.SetValue("System.AlarmLight", System.Drawing.Color.MediumBlue);
```

#### value="Folder"

```c#
source.SetValue("System.LogsFolder", "D:\\");
```

#### value="File"

```c#
source.SetValue("System.DataReport", "D:\\data.xlsx");
```

> 每次调用SetValue都会写XML文件到磁盘一次。如果有多个修改，单次批量提交性能更高开销更小，`SetValue(params (string configItem, object value)[] configValuePairs)`可以传入多个修改项，且可以保证只要有一项校验失败，则全部的设置项都不会被修改，即原子操作。

## ValueSet

调用SetValue( )会触发ValueSet事件，事件参数是被修改的配置项，旧值，新值。

主要作用是日志追溯。

```c#

source.ValueSet += Source_ValueSet;

// oldValue和newValue两个字符串肯定不同。因为SetValue()检测到新值和旧值相等的情况下会提前返回，不会真的去修改值，更不会触发ValueSet事件。
// by the way,old value可能是10，new value是0x0A,虽然都表示十进制的值，但仍旧会触发事件，因为比较规则是比较数据项的值的文本形式。
private void Source_ValueSet((string configItem, string oldValue, string newValue)[] obj)
{
    foreach (var item in obj)
    {
        Console.WriteLine($"{item.configItem} changed from {item.oldValue} to {item.newValue}.");
    }
}
```

## Valid XML Schema Specification

### How to edit xml

xml文件固定初始模板如下，必须要有根节点root。

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>

</root>
```

可以添加配置项`System.CycleCount`

```c#
<?xml version="1.0" encoding="utf-8"?>
<root>
	<config name="System">
		<config name="CycleCount" value="3" type="Integer"/>
	</config>
</root>
```

下面的配置是==无效==的。root节点下必须是分类节点！

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
	<config name="CycleCount" value="3" type="Integer"/>
</root>
```

root下可以有多个分类节点。下面有两个配置项`System.CycleCount`和`SetUp.RemoteIpAddress`

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
	<config name="System">
		<config name="CycleCount" value="3" type="Integer"/>
	</config>
    <config name="SetUp">
		<config name="RemoteIpAddress" value="127.0.0.1" type="String"/>
	</config>
</root>
```

可以自由调整数据节点的路径。数据项RemoteIpAddress的路径被调整成`SetUp.Address.RemoteIpAddress`

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
	<config name="System">
		<config name="CycleCount" value="3" type="Integer"/>
	</config>
    <config name="SetUp">
        <config name="Address">
            <config name="RemoteIpAddress" value="127.0.0.1" type="String"/>
        </config>
	</config>
</root>
```

同一分类节点下的分类节点的name不能相同，必须保证唯一；数据节点之间的name也不能相同。



错误范例1：root下有2个System

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
	<config name="System">
		<config name="CycleCount" value="3" type="Integer"/>
	</config>
    <config name="System">
		<config name="RemoteIpAddress" value="127.0.0.1" type="String"/>
	</config>
</root>
```



错误范例2：System下有2个CycleCount

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
	<config name="System">
		<config name="CycleCount" value="3" type="Integer"/>
        <config name="CycleCount" value="127.0.0.1" type="String"/>
	</config>
</root>
```



正确范例3：System下有2个CycleCount

```c#
<?xml version="1.0" encoding="utf-8"?>
<root>
	<config name="System">
		<config name="CycleCount" value="3" type="Integer"/>
        <config name="CycleCount">
            <config name="RemoteIpAddress" value="127.0.0.1" type="String"/>
	    </config>
	</config>
</root>
```



正确范例4：CycleCount节点下有一个CycleCount，这是允许的。

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
	<config name="System">
        <config name="CycleCount">
            <config name="RemoteIpAddress" value="127.0.0.1" type="String"/>
            <config name="CycleCount" value="3" type="Integer"/>
	    </config>
	</config>
</root>
```

==终极原则是：同一个分类节点下的直接子分类节点的Name不能重复，子数据节点的Name不能重复，进而保证每一个数据项的路径在全部数据项中都是唯一的。==

### Category node

> 不可省略的意思是，必须在xml中config指定该属性且值不能是空白。可省略的意思是可以在xml的config不写该属性或值是空白字符。
>
> 红色标记的属性是用于做可视化修改值的UI时使用的，如果只是在后台使用简单的读写功能，则无需理会这些属性。

`<config name="System">`

支持以下2个属性

- ==name== 分类节点标识，不可省略
- `display` 可省略，默认值与name相同。
- `visible` 节点下的数据项是否在UI可见。可省略，默认值是true。
- `Enable` 节点下的数据项是否可在UI更改。可省略，默认值是true。

### Data node

- ==name== 数据节点的标识。不可省略，必须保证在同一个父类别节点下的所有节点之间唯一。
- ==value==  数据项的值。不可省略。
- ==type== 数据项的值类型，枚举：
  - `Boolean`   value文本可被解读成C#的bool类型。

  - `Integer`    value文本可被解读成C#的byte，short，int，long，double，float，decimal。

  - `Decimal`   value文本可被解读成C#的double，float，decimal。

  - `String`  value文本可被解读成C#的string。
- ==max== 数据项的允许值的最大边界。此属性只有`type`是`Integer` `Decimal`才有效。可省略，默认值是decimal.MaxValue。
- ==min== 数据项的允许值的最小边界。此属性只有`type`是`Integer` `Decimal`才有效。可省略，默认值是decimal.MinValue。
- ==options== 允许的取值集合。用; 隔开，如 options="COM1;COM2;COM3" .  SetValue(string config，object newValue)，会先把newValue转换成type指定的类型，再将options的每一项转换成type指定的类型，这时候才开始检查options中是否有元素等于newValue，如果没有，会拒绝本次修改。举例：newValue是字符串"0xA"，Options字符串列表是["1","10","100"],设置值操作会成功，字符串"0xA"转换成整数是10，字符串"10"转换成整数是10，所以Options包含"0xA"。
- ==regex== 正则表达式。SetValue(string config，object newValue),`newValue.ToString()`字符串必须匹配此正则表达式，否则拒绝本次修改。
- ==regexNote== ==regex的注解==。正则表达式难以理解，可以用此项做注解，不是必须的。在程序中设置值时，如果正则表达式校验失败，抛出的异常信息是regexNote,如果未配置regexNote，异常信息是regex.
- `display` 可省略，默认值与name相同。
- `description` 数据项的描述。可省略，默认值是string.Empty。
- `unit` 数据项的单位，如 kg，Torr，mm，℃ ...... 可省略，默认值是string.Empty。
- `visible` 数据项是否在UI可见。可省略，默认值是true。
- `Enable` 数据项是否可在UI更改。可省略，默认值是true。
- `restart` 修改数据项的值后是否需要重启App才生效。可省略，默认值是false。

#### options
假设设置的新值是object obj，

type是ValueType.String时，匹配规则是 `options.Contains(obj.ToString())`

type是ValueType.Integer时，匹配规则是（ValueType.Decimal时类似）

```c#
var longOptions = new List<long>();
foreach (var option in options)
{
    if (TryParse2Long(option, out long longValue))
    {
        longOptions.Add(longValue);
    }
    else
    {
        throw new ConfigException($"option '{option}' can't convert to a integer for '{configItem}'.");
    }
}
TryParse2Long(strValue, out long @long);
if (!longOptions.Contains(@long))
{
    throw new ArgumentOutOfRangeException(nameof(value), $"The value '{strValue}' is not in the options for config item '{configItem}'.");
}
```

`Regex.IsMatch(decimal.Parse(obj).ToString(), regex)`

> type是ValueType.Boolean时，options决定true和false时在UI显示的文本。如options="on;off"，true显示on，false显示off。如果不需要UI界面，无需为ValueType.Boolean配置options。

#### regex

假设设置的新值是object obj，

type是ValueType.String时，匹配规则是 `Regex.IsMatch(obj.ToString(), regex)`

type是ValueType.Integer或ValueType.Decimal时，匹配规则是 `Regex.IsMatch(decimal.Parse(obj).ToString(), regex)`，如`^([02468]|[1-9]\d*[02468])$`,限制是偶数。

## CustomizeConfigItemValidationRule & CustomizeConfigItemOptionsSource

### Derived from PrimitiveConfigSource

PrimitiveConfigSource有两个虚方法

`CustomizeConfigItemValidationRule` 可以定制数据项的校验规则。\<config>节点只能用min,max,regex来校验，当这3个手段无法满足校验需求时，可以通过重写此方法再额外添加一些校验规则。

`CustomizeConfigItemOptionsSource` 可以定制数据项的限定值选项。\<config>节点可以用options来限定数据项允许的取值集合，当options无法很好的指定集合时，可以通过重写此方法指定允许的取值集合。注意：此方法返回的集合会覆盖掉\<config>的options，即导致xml中的options无效。



用法示例如下。

```c#
public class AppConfigSource : PrimitiveConfigSource
{
    public AppConfigSource(string xmlString) : base(xmlString)
    {
    }

    public AppConfigSource(string xmlDocumentPath, Encoding encoding) : base(xmlDocumentPath, encoding)
    {
    }

    protected override string[] CustomizeConfigItemOptionsSource(string configItem)
    {
        switch (configItem)
        {
            case "System.CycleCount":
                return ["1","2","3","4","5","6"];
            case "System.RemoteIpAddress":
                return ["127.0.0.1","192.168.2.22"];
            default:
                return null;
        }
    }

    protected override Func<string, bool> CustomizeConfigItemValidationRule(string configItem)
    {
        switch (configItem)
        {
            case "System.CycleCount":
                return (textValue) =>
                {
                    if (textValue.Any(x => !char.IsDigit(x)))
                    {
                        return false;
                    }
                    return true;
                };
            case "System.RemoteIpAddress":
                return (textValue) =>
                {
                    if(!Regex.IsMatch(textValue, "^((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}$"))
                    {
                        return false;
                    }
                    return true;
                };
            default:
                return null;
        }
    }
}
```

### innerconfig

xml的root节点下可以塞入多个innerconfig节点，它无法通过GetValue和SetValue两个API访问，只能通过`protected string GetInnerConfigValue(string name)`读取值，并没有提供修改值的api。

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
    <innerconfig name="EquipmentModel" value="ICP"/>
	<config name="System">
		<config value="3" name="CycleCount" type="Integer"/>
		<config value="false" name="IsSimulatorMode" type="Boolean"/>
		<config name="SetUp">
			<config value="5" name="DiskFreeSpaceAlarmTolerance" type="Decimal"/>
			<config value="127.0.0.1" name="RemoteIpAddress" type="String"/>
		</config>
	</config>
</root>
```

读取方法

```c#
var model = GetInnerConfigValue("EquipmentModel");
```

其主要用意是，同一个软件，配置项的校验规则或允许值集合不尽相同，可以同一份软件，兼容多种需求，不需要再开多个代码分支和版本，导致维护困难。

```c#
protected override Func<string, bool> CustomizeConfigItemValidationRule(string configItem)
{
    switch (configItem)
    {
        case "System.CycleCount":
            return (textValue) =>
            {
                if(GetInnerConfigValue("EquipmentModel") == "ICP")
                {
                    if (textValue.Any(x => !char.IsDigit(x)))
                    {
                        return false;
                    }
                    return true;
                }
                else if(GetInnerConfigValue("EquipmentModel") == "CCP")
                {
                    if (textValue.Any(x => !char.IsDigit(x)) && int.Parse(textValue) > 100)
                    {
                        return false;
                    }
                    return true;
                }
                return true;
            };
        default:
            return null;
    }
}
```

## Restarting app will restore the default value

PrimitiveConfigSource第2个构造方法如下

`public PrimitiveConfigSource(string xmlString)`

可以在内存中提供一个作为默认配置的xml文本，在软件启动后可以正常读取和修改配置项的值，但是软件重启后，所有配置又恢复默认配置，上一次软件运行修改的值被丢弃。也就是说，每次软件启动，使用的都是相同的初始配置。

```c#
string xmlString = """
    <?xml version="1.0" encoding="utf-8"?>
    <root>
    	<config name="System">
    		<config value="3" name="CycleCount" type="Integer"/>
    		<config value="false" name="IsSimulatorMode" type="Boolean"/>
    	</config>
    </root>
    """;

using PrimitiveConfigSource source = new PrimitiveConfigSource(xmlString);

Console.WriteLine(source.GetValue<int>("System.CycleCount"));
Console.WriteLine(source.GetValue<bool>("System.IsSimulatorMode"));

source.SetValue("System.CycleCount", 111);
source.SetValue("System.IsSimulatorMode", true);

Console.WriteLine(source.GetValue<int>("System.CycleCount"));
Console.WriteLine(source.GetValue<bool>("System.IsSimulatorMode"));

// PrimitiveConfigSource source = new PrimitiveConfigSource(File.ReadAllText("SetUp.xml")); // SetUp.xml作为软件初启动的默认配置，存放在应用程序目录下，可根据需要编辑修改。
```

## TODO
