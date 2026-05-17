using Repository.Entities;

namespace Service.Interfaces
{ 
    /// <summary>
    /// Defines the operations for the classification engine.
    /// </summary>
    public interface IClassificationService
    {
        Category PredictCategory(Dictionary<Tag, int> songTags);
    }
}
