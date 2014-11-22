hxm-heart-rate-sensor
=====================

This is a homebrew project for Zephyr HxM heart rate sensor for Windows (Phone) 8.1 XAML and Windows Phone 8.1 SL.
This only an example how you can connect to a Zephyr HxM BT heart rate sensor (non BT LE) and does not represent best practices.
It uses some basics of dependency injections and MVVM for demonstrating how can you separate UI and code in your project. (no external libraries)
This may or may not work properly. You're free to copy this code and/or modify it. Some credits are very welcome. :)

PS: Use MVVM framework of your choice in your projects. I just wanted to have a NuGet-free sample.

To make Windows (Phone) 8.1 project work with BT devices, you need to open Package.appxmanifest as code (F7 or right click -> View Code) and add capabilities inin <Capabilities>.

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

  Additionally Windows Phone 8.1 SL project requires ID_CAP_NETWORKING and ID_CAP_PROXIMITY in WMAppMenifest.xml. (under Properties folder)