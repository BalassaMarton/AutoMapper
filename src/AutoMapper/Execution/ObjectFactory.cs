using System.Collections.Immutable;
using System.Collections.ObjectModel;
namespace AutoMapper.Execution;
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ObjectFactory
{
    static readonly Expression EmptyString = Constant(string.Empty);
    static readonly LockingConcurrentDictionary<Type, Func<object>> CtorCache = new(GenerateConstructor);

    public static object CreateInstance(Type type) => CtorCache.GetOrAdd(type)();
    private static Func<object> GenerateConstructor(Type type) =>
        Lambda<Func<object>>(GenerateConstructorExpression(type, null).ToObject()).Compile();
    public static object CreateInterfaceProxy(Type interfaceType) => CreateInstance(ProxyGenerator.GetProxyType(interfaceType));
    public static Expression GenerateConstructorExpression(Type type, IGlobalConfiguration configuration) => type switch
    {
        { IsValueType: true } => configuration.Default(type),
        Type stringType when stringType == typeof(string) => EmptyString,
        Type immutableCollectionType when immutableCollectionType.IsImmutableCollection() => CreateImmutableCollection(type),
        { IsInterface: true } => CreateInterfaceExpression(type),
        { IsAbstract: true } => InvalidType(type, $"Cannot create an instance of abstract type {type}."),
        _ => CallConstructor(type, configuration)
    };
    private static Expression CallConstructor(Type type, IGlobalConfiguration configuration)
    {
        var defaultCtor = type.GetConstructor(Internal.TypeExtensions.InstanceFlags, []);
        if (defaultCtor != null)
        {
            return New(defaultCtor);
        }
        var ctorWithOptionalArgs =
            (from ctor in type.GetDeclaredConstructors() let args = ctor.GetParameters() where args.All(p => p.IsOptional) select (ctor, args)).FirstOrDefault();
        if (ctorWithOptionalArgs.args == null)
        {
            return InvalidType(type, $"{type} needs to have a constructor with 0 args or only optional args. Validate your configuration for details.");
        }
        var arguments = ctorWithOptionalArgs.args.Select(p => p.GetDefaultValue(configuration));
        return New(ctorWithOptionalArgs.ctor, arguments);
    }

    private static Expression CreateImmutableCollection(Type type) =>
        type.IsGenericType(typeof(ImmutableArray<>)) ? CreateFromStaticMember(type, EmptyImmutableArray) :
        type.IsGenericType(typeof(IImmutableList<>)) ? CreateFromStaticMember(type, EmptyImmutableList) :
        type.IsGenericType(typeof(ImmutableList<>)) ? CreateFromStaticMember(type, EmptyImmutableList) :
        type.IsGenericType(typeof(IImmutableSet<>)) ? CreateFromStaticMember(type, EmptyImmutableHashSet) :
        type.IsGenericType(typeof(ImmutableHashSet<>)) ? CreateFromStaticMember(type, EmptyImmutableHashSet) :
        type.IsGenericType(typeof(IImmutableQueue<>)) ? CreateFromStaticMember(type, EmptyImmutableQueue) :
        type.IsGenericType(typeof(ImmutableQueue<>)) ? CreateFromStaticMember(type, EmptyImmutableQueue) :
        type.IsGenericType(typeof(IImmutableStack<>)) ? CreateFromStaticMember(type, EmptyImmutableStack) :
        type.IsGenericType(typeof(ImmutableStack<>)) ? CreateFromStaticMember(type, EmptyImmutableStack) :
        type.IsGenericType(typeof(IImmutableDictionary<,>)) ? CreateFromStaticMember(type, EmptyImmutableDictionary) :
        type.IsGenericType(typeof(ImmutableDictionary<,>)) ? CreateFromStaticMember(type, EmptyImmutableDictionary) :
        InvalidType(type, $"Cannot create an instance of type {type}.");

    private static Expression CreateInterfaceExpression(Type type) =>
        type.IsGenericType(typeof(IDictionary<,>)) ? CreateCollection(type, typeof(Dictionary<,>)) :
        type.IsGenericType(typeof(IReadOnlyDictionary<,>)) ? CreateReadOnlyDictionary(type.GenericTypeArguments) :
        type.IsGenericType(typeof(ISet<>)) ? CreateCollection(type, typeof(HashSet<>)) :
        type.IsCollection() ? CreateCollection(type, typeof(List<>), GetIEnumerableArguments(type)) :
        InvalidType(type, $"Cannot create an instance of interface type {type}.");

    private static Type[] GetIEnumerableArguments(Type type) => type.GetIEnumerableType()?.GenericTypeArguments ?? [typeof(object)];
    private static Expression CreateCollection(Type type, Type collectionType, Type[] genericArguments = null) =>
        ToType(New(collectionType.MakeGenericType(genericArguments ?? type.GenericTypeArguments)), type);
    private static Expression CreateReadOnlyDictionary(Type[] typeArguments)
    {
        var ctor = typeof(ReadOnlyDictionary<,>).MakeGenericType(typeArguments).GetConstructors()[0];
        return New(ctor, New(typeof(Dictionary<,>).MakeGenericType(typeArguments)));
    }

    private static MemberInfo EmptyImmutableArray = Internal.TypeExtensions.GetMemberOfGenericTypeDefinition(() => ImmutableArray<object>.Empty);
    private static MemberInfo EmptyImmutableList = Internal.TypeExtensions.GetMemberOfGenericTypeDefinition(() => ImmutableList<object>.Empty);
    private static MemberInfo EmptyImmutableHashSet = Internal.TypeExtensions.GetMemberOfGenericTypeDefinition(() => ImmutableHashSet<object>.Empty);
    private static MemberInfo EmptyImmutableQueue = Internal.TypeExtensions.GetMemberOfGenericTypeDefinition(() => ImmutableQueue<object>.Empty);
    private static MemberInfo EmptyImmutableStack = Internal.TypeExtensions.GetMemberOfGenericTypeDefinition(() => ImmutableStack<object>.Empty);
    private static MemberInfo EmptyImmutableDictionary = Internal.TypeExtensions.GetMemberOfGenericTypeDefinition(() => ImmutableDictionary<object, object>.Empty);

    private static Expression CreateFromStaticMember(Type type, MemberInfo genericMember) =>
        ToType(
            MakeMemberAccess(null, genericMember.DeclaringType.MakeGenericType(type.GetGenericArguments()).GetMemberOfClosedGenericType(genericMember)),
            type);

    private static Expression InvalidType(Type type, string message) => Throw(Constant(new ArgumentException(message, "type")), type);
}