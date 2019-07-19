# Modbus TCP Master Example CSharp

A basic Modbus TCP Master example written in CSharp using the [CAS Modbus Stack](https://store.chipkin.com/services/stacks/modbus-stack)

## User input

- **Q** - Quit
- **R** - Read 40001, fnk=3, add=1, count=3
- **W** - Write 40001, fnk=16, add=1, count=3

## Example output

```txt
Starting Modbus TCP Master Example  version 0.0.1.0
https://github.com/chipkin/ModbusTCPMasterExampleCSharp
FYI: CAS Modbus Stack version: 2.3.11.0
FYI: CAS Modbus Stack Setup, successfuly

Key R pressed.
ReadExample
FYI: Connected to IP address=[192.168.1.26:502]
Sending ReadRegisters message
FYI: Sending 12 bytes to 192.168.1.26:502
00 00 00 00 00 06 FF 03 00 01 00 03
FYI: Recived 15 bytes from 192.168.1.26:502
00 00 00 00 00 09 00 03 06 00 01 00 02 00 03
Data: 1, 2, 3,

Key R pressed.
ReadExample
FYI: Connected to IP address=[192.168.1.26:502]
Sending ReadRegisters message
FYI: Sending 12 bytes to 192.168.1.26:502
00 00 00 00 00 06 FF 03 00 01 00 03
FYI: Recived 15 bytes from 192.168.1.26:502
00 00 00 00 00 09 00 03 06 00 01 00 02 00 03
Data: 1, 2, 3,

FYI: Key W pressed.
FYI: WriteExample
FYI: Connected to IP address=[192.168.1.26:502]
Sending WriteRegisters message
FYI: Sending 25 bytes to 192.168.1.26:502
00 00 00 00 00 13 FF 10 00 01 00 06 0C 00 63 00 96 01 32 00 07 BC F4 3A 4B
FYI: Recived 12 bytes from 192.168.1.26:502
00 00 00 00 00 06 00 10 00 01 00 06
Write was successful.

FYI: Key R pressed.
FYI: ReadExample
FYI: Connected to IP address=[192.168.1.26:502]
FYI: Sending ReadRegisters message
FYI: Sending 12 bytes to 192.168.1.26:502
00 00 00 00 00 06 FF 03 00 01 00 03
FYI: Recived 15 bytes from 192.168.1.26:502
00 00 00 00 00 09 00 03 06 00 63 00 96 01 32
Data: 99, 150, 306,

FYI: Key R pressed.
FYI: ReadExample
FYI: Connected to IP address=[192.168.1.26:502]
FYI: Sending ReadRegisters message
FYI: Sending 12 bytes to 192.168.1.26:502
00 00 00 00 00 06 FF 03 00 01 00 03
FYI: Recived 15 bytes from 192.168.1.26:502
00 00 00 00 00 09 00 03 06 00 63 00 96 01 32
Data: 99, 150, 306,
```

## Building

1. Copy *CASModbusStack_Win32_Debug.dll*, *CASModbusStack_Win32_Release.dll*, *CASModbusStack_x64_Debug.dll*, and *CASModbusStack_x64_Release.dll* from the [CAS Modbus Stack](https://store.chipkin.com/services/stacks/modbus-stack) project  into the /bin/ folder.
2. Use [Visual Studios 2019](https://visualstudio.microsoft.com/vs/) to build the project. The solution can be found in the */ModbusTCPMasterExampleCSharp/* folder.

Note: The project is automaticly build on every checkin using GitlabCI.
