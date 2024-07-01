使用C#编写的魔兽世界钓鱼辅助

功能简单 只有设置声音阈值这一个设置

![image](https://github.com/lzxstruts/FishingAssistant/assets/129748020/6376e012-3ed5-49d9-8ce7-cdca07484fa8)

检测魔兽世界声音

支持多开

![image](https://github.com/lzxstruts/FishingAssistant/assets/129748020/65fae8b3-8bf0-43f3-8029-b01943ec14f0)


wlk怀旧服相关说明
![image](https://github.com/lzxstruts/FishingAssistant/assets/129748020/dfafb804-6272-474a-8b20-856c42ae6734)

由于怀旧服钓鱼抛竿有时候会很远
与目标互动可能与鱼漂互动不到

推荐找个可控制距离的钓点
例如达拉然喷泉
代码中加入多次抛竿 
SendKey(dhwnd, 49);
await Task.Delay(10);
SendKey(dhwnd, 49);
await Task.Delay(10);
SendKey(dhwnd, 49);
await Task.Delay(10);
SendKey(dhwnd, 49);
await Task.Delay(10);
SendKey(dhwnd, 49);

钓鱼使用宏
/cast [nochanneling]钓鱼

