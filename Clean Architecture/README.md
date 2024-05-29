# Шаблон для создания проекта по канонам Clean Architecture

## Этапы создания решения

```
$CA_SlnPath = "ca-test"
$CA_SlnName = "CATest"
$CA_Framework = "net8.0"
$CA_SDKVersion = "8.0.100"

mkdir $CA_SlnPath

dotnet new globaljson --sdk-version $CA_SDKVersion --roll-forward latestFeature -o $CA_SlnPath
dotnet new sln -o $CA_SlnPath -n $CA_SlnName
```

Прописать TargetFramework для Directory.Build.props

```
<TargetFramework>net6.0</TargetFramework>
```

Нужные версии пакетов прописать в Directory.Packages.props
Пример:

```
<PackageVersion Include="Newtonsoft.Json" Version="13.0.1" />
```

Скопировать ассеты в папку с проектом и добавить их в решение

```
copy assets/* $CA_SlnPath/
```

Добавить в файл решения (.sln)

```
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Solution Items", "Solution Items", "{E2DA20AA-28D1-455C-BF50-C49A8F831633}"
	ProjectSection(SolutionItems) = preProject
		.editorconfig = .editorconfig
		.gitignore = .gitignore
		Directory.Build.props = Directory.Build.props
		Directory.Packages.props = Directory.Packages.props
		global.json = global.json
	EndProjectSection
EndProject
```

Создаем проекты решения

```
dotnet new web -o $CA_SlnPath/src/Web --framework $CA_Framework
dotnet new classlib -o $CA_SlnPath/src/Application --framework $CA_Framework
dotnet new classlib -o $CA_SlnPath/src/Infrastructure --framework $CA_Framework
dotnet new classlib -o $CA_SlnPath/src/Domain --framework $CA_Framework
```

Во всех проектах удаляем PropertyGroup, так как свойства перенесены в Directory.Build.props

```
<PropertyGroup>
  <TargetFramework>net6.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

В каждый проект добавляем RootNamespace и AssemblyName

```
<PropertyGroup>
  <RootNamespace>$CA_SlnName.Web</RootNamespace>
  <AssemblyName>$CA_SlnName.Web</AssemblyName>
</PropertyGroup>
```

Добавляем проекты в решение и связываем проекты ссылками

```
dotnet sln $CA_SlnPath/$CA_SlnName.sln add (ls -r $CA_SlnPath/src/*.csproj)

dotnet add $CA_SlnPath/src/Web reference $CA_SlnPath/src/Application
dotnet add $CA_SlnPath/src/Web reference $CA_SlnPath/src/Infrastructure
dotnet add $CA_SlnPath/src/Infrastructure reference $CA_SlnPath/src/Application
dotnet add $CA_SlnPath/src/Application reference $CA_SlnPath/src/Domain
```

## TODO: Tests

Добавить проекты для тестов (см. clean-architecture):

* Application.FunctionTests
* Application.UnitTests
* Infrastructure.IntegrationTests
* Domain.UnitTests

Добавить референсы для тестов: ?