interface ICallable {
    int Arity();
    object Call(List<object> arguments);
}

class Callable(int arity, object call, string representation) : ICallable {
    public int Arity()
    {
        return arity;
    }

    public object Call(List<object> arguments)
    {
        return call;
    }

    public override string ToString() {
        return representation; // representation of the callable
    }
}

