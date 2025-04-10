## TODO: tests

Добавить проекты для тестов (см. clean-architecture):

* Application.FunctionTests
* Application.UnitTests
* Infrastructure.IntegrationTests
* Domain.UnitTests

Добавить референсы для тестов: ?

## TODO: Разобрать это

#### Domain
Не использовать Data Annotations в domain сущностях, использовать Fluent API\
Использовать Value Objects там где уместно\
Использовать пользовательские исключения\
Инициализировать все коллекции

#### Application
CQRS + MediatR\
Fluent Validation\
AutoMapper

#### Infrastructure
Независимость от базы данных\
Fluent API (data annotations)\
Там где возможно использовать conventions вместо configuration (меньше кода)\
Никакой из слоев не зависит от Infrastructure

#### Api (Web, UI и т.д.)
В контроллерах не должно быть никакой логики\
ViewModels\
Open API (swagger)
