using E2E.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E2E.Tests.Collections
{
    [CollectionDefinition("Docker collection")]
    public class DockerCollection : ICollectionFixture<DockerServerFixture>
    {
    }
}
