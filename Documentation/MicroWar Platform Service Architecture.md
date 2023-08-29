# MicroWar Platform Service Architecture

Considering the maintainability and scalability of the program, we have encapsulated the PICO Platform APIs. This encapsulation ensures relatively independent logic for each component while providing a unified access approach for all feature's instance. By utilizing an ***event dispatc system***, we have interconnected these components to reduce coupling between modules.

In this architecture, specific scripts implement the logic for different services. The PlatformServiceManager serves as an interface for accessing various instances. Here is the platform service structure diagram used in the MicroWar project:

![PICO Platform Service Structure](/Documentation/Files/PlatformServiceStructure.jpg)
## Event Dispatch
 **C# Code Example - Initializing the SDK**:
   
   ```csharp
   using PicoSDK;

   // Initialize the PICO SDK with your API key
   PicoSDK.Initialize("your_api_key");
   ```
## Get Service Instance

## More to Explore

For detailed implementation specifics and usage guidelines, please refer to the following documentation:
- [Rooms](/Documentation/Rooms.md)
- [RTC](/Documentation/RTC.md)
- [Multiplay](/Documentation/Multiplays.md)
