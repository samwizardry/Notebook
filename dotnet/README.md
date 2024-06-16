# dotnet

Чтобы включить библиотеки из AspNetCore, в проектах типа classlib, в .csproj нужно прописать.

```
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

