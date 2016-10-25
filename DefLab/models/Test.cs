using FluentNHibernate.Mapping;

namespace DefLab.models
{
    public class Test
    {
        public virtual long Id { get; protected set; }
        public virtual string Data { get; set; }
    }

    public class TestMap : ClassMap<Test>
    {
        public TestMap()
        {
            Id(x => x.Id);
            Map(x => x.Data);
        }
    }
}
