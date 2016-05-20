namespace Unity.Patterns
{
    public interface IMyService
    {
    }

    public class MyService : IMyService
    {
    }

    public class MyServiceDecorator1 : IMyService
    {
        public readonly IMyService Inner;

        public MyServiceDecorator1(IMyService inner)
        {
            Inner = inner;
        }
    }

    public class MyServiceDecorator2 : IMyService
    {
        public readonly IMyService Inner;

        public MyServiceDecorator2(IMyService inner)
        {
            Inner = inner;
        }
    }
}