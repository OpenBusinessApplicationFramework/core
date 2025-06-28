# Core

The goal of this open source project is to make application development—especially in business environments—easier, faster, and more straightforward. The ultimate objective is that anyone with limited coding knowledge can create their own application in JavaScript with minimal effort. Additionally, the GUI should be auto-generated.

## Table of Contents

- [Development Status](#development-status)
- [Detailed Roadmap](#detailed-roadmap)
- [Contributing](#contributing)
- [Third-Party Licenses](#third-party-licenses)
- [License](#license) 

## Development Status

> &#x26A0; **In Development – Not Ready for Production**

This project is currently under active development. Please note the following:

- **Tenancy feature not working**  
  The tenancy feature exists only as a placeholder and has no effect.
  
- **Insufficient test coverage**  
  More tests are planned; at the moment, only the most critical areas are covered.

- **Incomplete documentation**  
  No documentation is available at the moment; it will be provided later.

- **Some important security features are missing**  
  Features such as OIDC and a Key Vault are currently absent but will be implemented in the future.

- **Missing UI**  
  No GUI has been implemented yet. The goal is to have an admin GUI and a dynamically generated GUI for the applications built with this library.

## Detailed Roadmap

| Milestone                             | Target Date    | Status              |
|---------------------------------------|---------------:|:-------------------:|
| Conceptual design                     | Late Q1 2025   | &#x2705; Done       |
| API Completion (v1)                   | Q2 2025        | &#x2705; Done       |
| Tenancy feature                       | Early Q3 2025  | &#x2699; In Progress|
| GUI Alpha (Auto-GUI)                  | Q3 2025        | &#x2699; In Progress|
| OIDC & Key Vault Integration          | Q3 2025        | &#x23F3; Upcoming   |
| Admin tool                            | Late Q3 2025   | &#x23F3; Upcoming   |
| Migration to .NET 10                  | Q4 2025        | &#x23F3; Upcoming   |
| Public Beta & Feedback Round          | Q4 2025        | &#x23F3; Upcoming   |
| Full test coverage                    | Until Q4 2025  | &#x23F3; Upcoming   |
| Full Documentation & Tutorials        | Late Q4 2025   | &#x23F3; Upcoming   |
| Release                               | Late Q4 2025   | &#x23F3; Upcoming   |


## Contributing

We welcome all contributions! Feel free to:

- **Star** this repository ⭐ to show your support.
- **Fork** the project, make your changes, and submit a Pull Request.
- **Open an Issue** to report bugs or suggest new features.

Thank you for helping!

## Third-Party Licenses

This project includes the following third-party dependencies. Please review their respective licenses before distributing or modifying:

- **Microsoft.AspNetCore.OData (v8.3.0)**  
  Licensed under the MIT License.  
  [View the full text](https://github.com/OData/AspNetCoreOData/blob/master/License.txt)

- **Npgsql.EntityFrameworkCore.PostgreSQL (v8.0.11)**  
  Licensed under the PostgreSQL License.  
  [View the full text](https://github.com/npgsql/efcore.pg/blob/main/LICENSE)

- **Swashbuckle.AspNetCore (v6.6.2)**  
  Licensed under the MIT License.  
  [View the full text](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/LICENSE)

- **Microsoft.EntityFrameworkCore.Design (v8.0.15)**  
  Licensed under the MIT License.  
  [View the full text](https://github.com/dotnet/efcore/blob/main/LICENSE.txt)

- **Jint (v4.2.2)**  
  Licensed under the BSD 2-Clause License.  
  [View the full text](https://github.com/sebastienros/jint/blob/main/LICENSE.txt)

- **IdGen (v3.0.7)**  
  Licensed under the MIT License.  
  [View the full text](https://github.com/RobThree/IdGen/blob/master/LICENSE)

## License

This project is licensed under the **LGPL-2.1 license**. See [LICENSE](LICENSE) for details.