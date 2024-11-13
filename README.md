# Cache Disk

The **Cache Disk** project is designed to create and manage a cache of a directory. The **Cache Disk** store a directory cache in a specific location, originally designed to be a SSD, while the original directory can be store in a HDD, allowing to work faster and to save or revert the original content.

> **NOTE:** This project is under development and some features may not be fully implemented or stable.

> The **Cache Disk** project development was temporary paused. The development is planned to restart in December of 2024
>
> **NOTE:** This project was developed under Windows OS until this moment with using the .NET 8. It is planned to add support to Unix systems when the development restart.

## Project Components:

The **Cache Disk** project is divided by three components:

| Component | Version | Status | Description |
| --------- | ------- | ------ | ----------- |
| Cache Disk Console | 0.1.0-alpha | in development | Console for cache management |
| Cache Disk Library | 0.8.2-beta | in development | The ***Cache Disk*** project. This library contains all methods, datatype for Console and PowerShell modules. |
| Cache Disk Module | 0.0.1-alpha | planned to develop after the console is stable | PowerShell module to use the terminal as a faster way to use create/save/revert operations to the cache |

## License

A copy is available in this [file](/LICENSE.txt)

MIT License

Copyright (c) 2023 - 2024 Matheus L. Silvati

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.