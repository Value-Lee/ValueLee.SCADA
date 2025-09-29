## Property List

| Property                    | Description                      |
| --------------------------- | -------------------------------- |
| DataGridWidth               | 配置项表格宽度                          |
| DescColumnMaxWidth          | 说明列最大宽度                          |
| DescColumnMinWidth          | 说明列最小宽度                          |
| DisableItems                | 禁止修改配置项                          |
| DisableNodes                | 禁止修改的分类节点，此节点下的所有配置项和节点都会被禁止修改   |
| HiddenItems                 | 隐藏的配置项                           |
| HiddenNodes                 | 隐藏的分类节点，此节点下的所有配置项和节点都会被隐藏       |
| IsDescriptionColumnVisible  | 是否显示说明列                          |
| IsExpandCollapseVisible     | 是否显示一键展开和一键折叠按钮                  |
| IsMaxColumnVisible          | 是否显示最大值限制列                       |
| IsMinColumnVisible          | 是否显示最小值列                         |
| IsNoColumnVisible           | 是否显示序号列                          |
| IsSearchBoxVisible          | 是否显示搜索栏                          |
| IsTreeViewNavigationVisible | 是否显示左侧的搜索栏，展开折叠，节点               |
| IsUnitColumnVisible         | 是否显示单位列                          |
| MaxColumnMaxWidth           | 最大值限制列最小宽度                       |
| MaxColumnMinWidth           | 最大值限制列最大宽度                       |
| MinColumnMaxWidth           | 最小值限制列最小宽度                       |
| MinColumnMinWidth           | 最小值限制列最大宽度                       |
| NameColumnMaxWidth          | 配置名称列最大宽度                        |
| NameColumnMinWidth          | 配置名称列最小宽度                        |
| PrimitiveConfigSource       | 关联的PrimitiveConfigSource，控件的核心属性 |
| SetpointColumnMaxWidth      | 设置列最大宽度                          |
| SetpointColumnMinWidth      | 设置列最小宽度                          |
| TextBoxTemplateDirtyBrush   | 设置列文本框有文本输入后背景色                  |
| TextBoxTemplateErrorBrush   | 设置列文本框输入非法的背景色                   |
| TreeViewNavigationMaxWidth  | 导航栏最大宽度                          |
| TreeViewNavigationMinWidth  | 导航栏最小宽度                          |
| UnitColumnMaxWidth          | 单位列最大宽度                          |
| UnitColumnMinWidth          | 单位列最小宽度                          |
| ValueColumnMaxWidth         | 当前值列最大宽度                         |
| ValueColumnMinWidth         | 当前值列最小宽度                         |

## Localization

```c#
public partial class App : Application
{
    public App()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
        // CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("zh-CN");
        // CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("zh-CN");
    }
}
```

## Batch Submission

修改配置项后，会直接提交到本地文件。可以提取XML文件的内容，作为内存形式的PrimitiveConfigSource，然后提交都提交到内存，通过ValueSet事件缓存修改项，然后最后可以批量提交缓存的变化。

## Hide & Disable - Items & Nodes

```C#
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

















