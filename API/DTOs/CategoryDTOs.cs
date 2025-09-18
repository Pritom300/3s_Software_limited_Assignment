namespace API.DTOs
{
    public class CategoryCreateDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    public class CategoryReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int ProductCount { get; set; }
    }

    public class CategoryUpdateDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
