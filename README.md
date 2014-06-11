hxm-heart-rate-sensor
=====================

This is a homebrew project for Zephyr HxM heart rate sensor.
This only an example how you can connect to a Zephyr HxM heart rate sensor (non BT LE) and does not represent best practices.
It uses some basics of dependency injections and MVVM for demonstrating how can you separate UI and code in your project.
This may or may not work properly. You're free to copy this code and/or modify it. Some credits are very welcome. :)

Current version supports only WP 8.1 XAML and Windows 8.1.
Next version will have more comment and Windows Phone 8.0 support.

To make Windows (Phone) 8.1 project work with BT devices, you need to open Package.appxmanifest as code (F7 or right click -> View Code) and add this lines in <Capabilities> element:
    <m2:DeviceCapability Name="bluetooth.rfcomm">
      <m2:Device Id="any">
        <m2:Function Type="name:serialPort" />
      </m2:Device>
    </m2:DeviceCapability>

This is what I used in my projects:
  <Capabilities>
    <Capability Name="internetClientServer" />

    <!-- This is used for BT connection -->
    <m2:DeviceCapability Name="bluetooth.rfcomm">
      <m2:Device Id="any">
        <m2:Function Type="name:serialPort" />
      </m2:Device>
    </m2:DeviceCapability>
  </Capabilities>