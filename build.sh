#!/usr/bin/env bash

dotnet tool restore
dotnet paket install
dotnet build

# cd ./demos/counter
# yarn install --pure-lockfile
# yarn webpack
# cd -

# cd ./demos/todos
# yarn install --pure-lockfile
# yarn webpack
# cd -
