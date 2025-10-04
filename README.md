# NtDriverTool

## Overview

NtDriverTool is a comprehensive Windows driver management utility that provides advanced control and visibility of all drivers installed on your system. It offers capabilities beyond what standard Windows tools provide, allowing both administrators and developers to manage, inspect, and troubleshoot kernel drivers.

## Features

- **Comprehensive Driver Visibility**: View all drivers on the system including:

    - Active drivers loaded in the kernel 
    - Configured but unloaded drivers
    - Drivers with registry entries but unknown to Service Control Manager
    - "Hidden" drivers with kernel objects but no registry presence
  
- **Advanced Driver Management**:

    - Start/stop driver services via Service Control Manager
    - Load/unload drivers using native Windows APIs (ZwLoadDriver/ZwUnloadDriver)
    - Create key and unload drivers with no registry key
    - Create, edit, and delete driver registry entries

- **Detailed Information Display**:

    - Driver status (Running, Stopped, etc.)
    - Image paths and load addresses
    - Module details (size, flags, load count)
    - Registry configuration
    - Driver object information

- **User-Friendly Interface**:

    - Customizable column display
    - Type-ahead search to quickly locate drivers
    - Context menus for common operations
    - One-click refresh (F5)

## Requirements

- Windows 7 or later
- Administrator privileges for any driver management operations
- .NET Framework 4.7.2 or newer

## Installation

1. Download the latest release from the Releases page
2. Extract the ZIP file to your preferred location
3. Run NtDriverTool.exe
4. No installation is required as the application is portable.

## Usage Guide

### Basic Operations

- **View Driver Information**: Launch the application to see a list of all detected drivers
- **Refresh Driver List**: Click the Refresh button or press F5
- **Search for a Driver**: Start typing the driver name when the grid is in focus
- **Customize View**: Right-click on column headers to select which columns to display

### Managing Drivers

- **Service Operations**: Right-click on a driver and select:

    - "Start Service" to start a stopped driver service
    - "Stop Service" to stop a running driver service

- **Driver Loading**: Right-click on a driver and select:

    - "ZwLoadDriver" to load an unloaded driver
    - "ZwUnloadDriver" to unload a loaded driver
    - "ZwUnloadDriver (Force)" for drivers with no registry key

- **Registry Operations**: Right-click on a driver and select:

    - "Create Key" to create a new registry entry
    - "Edit Service" to modify an existing entry 
    - "Delete Key" to remove a registry entry

- **View Driver Details**: Right-click and select "Details" for comprehensive information

### Creating New Drivers

1. Click the "Create Key" button
2. Enter a unique service name
3. Configure the driver properties in the dialog:

    - **Image Path**: Location of the driver file
    - **Type**: Select the driver type (Kernel, FileSystem, etc.)
    - **Start Type**: When the driver should load
    - **Error Control**: How to handle loading errors

## Technical Details

NtDriverTool uses a combination of Windows NT native APIs and standard Windows service interfaces to provide comprehensive driver management:

- SystemModuleInformation for enumerating loaded drivers
- Registry access for configured drivers
- Service Control Manager for service operations
- ZwLoadDriver / ZwUnloadDriver for loading/unloading drivers manually

## License

NtDriverTool is released under the MIT License. See LICENSE for details.

## Acknowledgements

- Inspired by tools like DriverView and System Informer
- Utilizing NtCoreLib by James Forshaw for low-level NT API access
- Mostly written by Claude Sonnet 3.7 Thinking
