using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Core.Specs;
using Catalog.Infrastructure.Data;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Repositories;

public class ProductRepository(ICatalogContext context) : IProductRepository, IBrandRepository, ITypesRepository
{
    private readonly ICatalogContext _context = context;
    private async Task<IReadOnlyList<Product>> _dataFilter(CatalogSpecParams catalogSpecParams, FilterDefinition<Product> filter)
    {
        var sortDefinition = Builders<Product>.Sort.Ascending(nameof(Product.Name)); //Default
        if (!string.IsNullOrEmpty(catalogSpecParams.Sort))
        {
            switch (catalogSpecParams.Sort)
            {
                case "priceAsc":
                    sortDefinition = Builders<Product>.Sort.Ascending(nameof(Product.Price));
                    break;
                case "priceDesc":
                    sortDefinition = Builders<Product>.Sort.Descending(nameof(Product.Price));
                    break;
                default:
                    sortDefinition = Builders<Product>.Sort.Ascending(nameof(Product.Name));
                    break;
            }
        }
            
        return await _context.Products
            .Find(filter)
            .Sort(sortDefinition)
            .Skip((catalogSpecParams.PageIndex -1) * catalogSpecParams.PageSize)
            .Limit(catalogSpecParams.PageSize)
            .ToListAsync();
    }
    public async Task<Pagination<Product>> GetProducts(CatalogSpecParams catalogSpecParams)
    {
        var builder = Builders<Product>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrEmpty(catalogSpecParams.Search))
            filter = filter & builder.Where(p => p.Name.ToLower().Contains(catalogSpecParams.Search.ToLower()));
        if (!string.IsNullOrEmpty(catalogSpecParams.BrandId))
            filter = filter & builder.Eq(p => p.Brands.Id, catalogSpecParams.BrandId);
        if (!string.IsNullOrEmpty(catalogSpecParams.TypeId))
            filter = filter & builder.Eq(p => p.Types.Id, catalogSpecParams.TypeId);
        
        var totalItems = await _context.Products.CountDocumentsAsync(filter);
        var data = await _dataFilter(catalogSpecParams, filter);

        return new Pagination<Product>(catalogSpecParams.PageIndex, catalogSpecParams.PageSize, (int)totalItems, data);
    }
    public async Task<Product> GetProduct(string id)
    {
        return await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
    }
    public async Task<IEnumerable<Product>> GetProductByName(string name)
    {
        return await _context.Products.Find(p => p.Name.ToLower().Contains(name.ToLower())).ToListAsync();
    }
    public async Task<IEnumerable<Product>> GetProductByBrand(string brandName)
    {
        return await _context.Products.Find(p => p.Brands.Name.ToLower() == brandName.ToLower()).ToListAsync();
    }
    public async Task<Product> CreateProduct(Product product)
    {
        await _context.Products.InsertOneAsync(product);
        return product;
    }
    public async Task<bool> UpdateProduct(Product product)
    {
        var updatedProduct = await _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
        return updatedProduct.IsAcknowledged && updatedProduct.ModifiedCount > 0;
    }
    public async Task<bool> DeleteProduct(string id)
    {
        var deletedProduct = _context.Products.DeleteOneAsync(p => p.Id == id);
        return deletedProduct.IsCompletedSuccessfully;
    }
    public async Task<IEnumerable<ProductBrand>> GetAllBrands()
    {
        return await _context.Brands.Find(p => true).ToListAsync();
    }
    public async Task<IEnumerable<ProductType>> GetAllTypes()
    {
        return await _context.Types.Find(p => true).ToListAsync();
    }
}