﻿using DalApi;
using Dal;
using BlApi;

namespace BlImplementation;
internal class BlProduct : BlApi.IProduct
{
    private IDal Dal = new DalList();

    /// <summary>
    /// A private help function for checking the integrity of the data in the logical layer for adding/updating a product.
    /// </summary>
    private void checkOrdValues(BO.Product prod)
    {
        if (prod.Id < 1)
        {
            throw new InvalidValue("ID");
        }
        if (string.IsNullOrEmpty(prod.Name))
        {
            throw new InvalidValue("name");
        }
        if (prod.Price < 0)
        {
            throw new InvalidValue("price");
        }
        if (prod.InStock < 0)
        {
            throw new InvalidValue("amount in stock");
        }
    }

    /// <summary>
    /// A private help function to convert DO.Product entity to BO.Product entity.
    /// </summary>
    private BO.Product convertDToB(DO.Product dP)
    {
        BO.Product bP = new BO.Product();
        bP.Id = dP.Id;
        bP.Name = dP.Name;
        bP.Price = dP.Price;
        bP.Category = (BO.eCategory)dP.category;
        bP.InStock = dP.InStock;
        return bP;
    }

    /// <summary>
    /// A function that receives product data, checks its integrity 
    /// and sends a request to the data layer to add such a product.
    /// </summary>
    public int CreateProd(BO.Product prod)
    {
        int id = 0;
        checkOrdValues(prod);
        DO.Product newProd = new DO.Product();
        newProd.Name = prod.Name;
        newProd.Price = prod.Price;
        newProd.category = (DO.eCategory)prod.Category;
        newProd.InStock = prod.InStock;
        bool tryId = true;
        while (tryId)//Create an ID
        {
            tryId = false;
            try
            {
                Random rnd = new Random();
                newProd.Id = rnd.Next(100000, 1000000);
                id = Dal.product.Create(newProd);
            }
            catch (IdAlreadyExists)
            {
                tryId = true;
            }
        }
        return id;
    }

    /// <summary>
    /// A function that receives a product ID, checks that it does not exist in orders 
    /// and sends a request to the data layer to delete it from the list
    /// </summary>
    public void DeleteProd(int Id)
    {
        IEnumerable<DO.OrderItem> orderItemsList = Dal.orderItem.Read();
        DO.OrderItem ordWithProd = orderItemsList.Where(o => o.ProductId == Id).FirstOrDefault();
        if (!ordWithProd.Equals(default(DO.OrderItem)))
        {
            throw new IllegalDeletion("It is not possible to delete an existing product in an order");
        }
        try
        {
            Dal.product.Delete(Id);
        }
        catch (IdNotExist)
        {
            throw new IllegalDeletion("A non-existent product cannot be deleted");
        }
    }
    /// <summary>
    /// A function that returns an entity to display product data for a customer screen 
    /// by referring to the data layer using an ID.
    /// </summary>
    public BO.ProductItem ReadProdCustomer(int Id, BO.Cart cart)
    {
        try
        {
            if (Id < 1)
            {
                throw new InvalidValue("ID");
            }
            DO.Product dP = Dal.product.Read(Id);
            BO.ProductItem bP = new();
            bP.Id = dP.Id;
            bP.Name = dP.Name;
            bP.Price = dP.Price;
            bP.Category = (BO.eCategory)dP.category;
            bP.InStock = (dP.InStock > 0) ? true : false;
            bool exist = false;
            foreach (BO.OrderItem i in cart.Items)
            {
                if (i.ProductId == bP.Id)//The product is already in the shopping cart
                {
                    exist = true;
                    bP.Amount = i.Amount;
                }
            }
            if(exist== false)
            {
                bP.Amount = 0;
            }
            return bP;
        }
        catch (IdNotExist exc)
        {
            throw new DataError(exc);
        }
    }

    /// <summary>
    /// A function that returns an entity to display product data for the manager screen 
    /// by referring to the data layer using an ID.
    /// </summary>
    public BO.Product ReadProdManager(int Id)
    {
        try
        {
            if (Id < 1)
            {
                throw new InvalidValue("ID");
            }
            DO.Product dP = Dal.product.Read(Id);
            return convertDToB(dP);
        }
        catch (IdNotExist exc)
        {
            throw new DataError(exc);
        }
    }

    /// <summary>
    /// A function to read the list of products
    /// </summary>
    public IEnumerable<BO.ProductForList> ReadProdsList()
    {
        IEnumerable<DO.Product> dProds = Dal.product.Read();
        IEnumerable<BO.ProductForList> bProds = new List<BO.ProductForList>(dProds.Count());
        foreach (DO.Product dP in dProds)
        {
            BO.ProductForList bP = new BO.ProductForList();
            bP.Id = dP.Id;
            bP.Name = dP.Name;
            bP.Price = dP.Price;
            bP.Category = (BO.eCategory)dP.category;
            bProds.Append(bP);
        }
        return bProds;
    }

    /// <summary>
    /// A function that receives product data, checks their integrity
    ///and sends a request to the data layer to update the product with such an ID.
    /// </summary>
    public void UpdateProd(BO.Product prod)
    {
        checkOrdValues(prod);
        DO.Product newProd = new DO.Product();
        newProd.Id = prod.Id;
        newProd.Name = prod.Name;
        newProd.Price = prod.Price;
        newProd.category = (DO.eCategory)prod.Category;
        newProd.InStock = prod.InStock;
        try
        {
            Dal.product.Update(newProd);
        }
        catch (IdNotExist err)
        {
            throw new DataError(err);
        }
    }

}