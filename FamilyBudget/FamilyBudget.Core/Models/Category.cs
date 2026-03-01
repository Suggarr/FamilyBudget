//using CSharpFunctionalExtensions;

//namespace FamilyBudget.Core.Models;

//public class Category
//{
//    public const int MAX_NAME_LENGTH = 100;

//    private Category(Guid id, Guid familyId, string name)
//    {
//        Id = id;
//        FamilyId = familyId;
//        Name = name;
//    }

//    public Guid Id { get; }
//    public Guid FamilyId { get; }
//    public string Name { get; }

//    public static Result<Category> Create(Guid id, Guid familyId, string name)
//    {
//        if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
//        {
//            return Result.Failure<Category>($"Category name can not be empty or longer than {MAX_NAME_LENGTH} symbols");
//        }

//        var category = new Category(id, familyId, name.Trim());
//        return Result.Success(category);
//    }
//}