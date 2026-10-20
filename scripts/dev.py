#!/usr/bin/env python3
"""Inicialização e comandos locais, sem imprimir credenciais ou carregar .env no shell."""
import argparse
import os
from pathlib import Path
import secrets
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
API = "src/Workoutly.Api/Workoutly.Api.csproj"
TESTS = "tests/Workoutly.Api.Tests/Workoutly.Api.Tests.csproj"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=["init", "migrate", "run", "test-db"])
    command = parser.parse_args().command
    if command == "init":
        password, root_password = secrets.token_hex(24), secrets.token_hex(24)
        content = (
            "MYSQL_PORT=3307\nMYSQL_DATABASE=workoutly\nMYSQL_USER=workoutly\n"
            f"MYSQL_PASSWORD={password}\nMYSQL_ROOT_PASSWORD={root_password}\n"
            "ConnectionStrings__Workoutly=Server=127.0.0.1;Port=3307;Database=workoutly;"
            f"User ID=workoutly;Password={password};SslMode=Disabled\n"
        )
        try:
            descriptor = os.open(ROOT / ".env", os.O_WRONLY | os.O_CREAT | os.O_EXCL, 0o600)
        except FileExistsError:
            print(".env já existe; nenhuma credencial foi alterada.")
            return 1
        with os.fdopen(descriptor, "w", encoding="utf-8") as file:
            file.write(content)
        print(".env criado com senhas aleatórias. Não versionar nem compartilhar esse arquivo.")
        return 0

    values = {}
    env_file = ROOT / ".env"
    if env_file.exists():
        for line in env_file.read_text(encoding="utf-8").splitlines():
            if line.strip() and not line.lstrip().startswith("#"):
                key, value = line.split("=", 1)
                values[key.strip()] = value.strip()
    environment = os.environ.copy()
    connection = environment.get("ConnectionStrings__Workoutly") or values.get("ConnectionStrings__Workoutly")
    if connection:
        environment["ConnectionStrings__Workoutly"] = connection
    local_dotnet = Path.home() / ".dotnet" / "dotnet"
    dotnet = environment.get("DOTNET_COMMAND") or (str(local_dotnet) if local_dotnet.is_file() else shutil.which("dotnet"))
    if not dotnet:
        print("SDK .NET não encontrado. Configure DOTNET_COMMAND.", file=sys.stderr)
        return 1
    if command in {"run", "migrate"}:
        if not connection:
            print("Defina ConnectionStrings__Workoutly ou prepare o .env; veja docs/mysql.md.", file=sys.stderr)
            return 1
        arguments = ["run", "--project", API, "--no-launch-profile"]
        arguments += ["--", "--migrate"] if command == "migrate" else ["--urls", "http://localhost:5080"]
    else:
        if not environment.get("WORKOUTLY_TEST_ADMIN_CONNECTION"):
            # Credencial administrativa apenas para o MySQL local e para bases aleatórias de teste.
            password = values.get("MYSQL_ROOT_PASSWORD")
            if not password:
                print("Prepare o .env ou defina WORKOUTLY_TEST_ADMIN_CONNECTION de um MySQL de testes isolado.", file=sys.stderr)
                return 1
            port = values.get("MYSQL_PORT", "3307")
            environment["WORKOUTLY_TEST_ADMIN_CONNECTION"] = (
                f"Server=127.0.0.1;Port={port};User ID=root;Password={password};SslMode=Disabled"
            )
        arguments = ["test", TESTS, "--logger", "console;verbosity=minimal"]
    return subprocess.call([dotnet, *arguments], cwd=ROOT, env=environment)


if __name__ == "__main__":
    sys.exit(main())
