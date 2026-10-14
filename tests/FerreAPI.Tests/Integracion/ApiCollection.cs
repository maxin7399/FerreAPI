namespace FerreAPI.Tests.Integracion;

[CollectionDefinition(Nombre)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Nombre = "Api";
}