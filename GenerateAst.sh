#!/bin/bash

SCRIPT_DIR=$(dirname $0)

dotnet run --project CraftingInterpreter.AstGenerator -- ${SCRIPT_DIR}/CraftingInterpreter/AbstractSyntaxTree/
