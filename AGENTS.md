# Contributor Guidelines

This repository contains standalone Unity scripts. Each script is intended to be
copied directly into a Unity project, so build artifacts or Unity project files
are not tracked here.

## Code Style
- Keep `using` directives grouped at the top of each file.
- Follow the existing formatting of braces and indentation.

## Before committing
- **Double check dependencies**. Make sure all required namespaces are imported
  via `using` statements. Missing `using` lines are the most common cause of
  compile errors when these scripts are dropped into a Unity project.
- Remove any unused `using` directives to keep the scripts clean.

There are currently no automated tests for this repository. Visual inspection is
important. When possible, try building a minimal Unity scene to confirm scripts
compile and behave as expected.
