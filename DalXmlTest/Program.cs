﻿/// <summary>
/// C# project Stage 1 
/// by Ozer Ester 214255705 and Ekshtein Chana 213868631
/// </summary>


using Dal;
using DalApi;
using DO;
using System.Xml.Linq;
using System.Xml.Serialization;

//=========================== Generic variables
char choice;
IDal? dalXml = DalXml.Instance;

XElement? products = XDocument.Load("..\\..\\..\\..\\xml\\product.xml").Root;
XElement? orders = XDocument.Load("..\\..\\..\\..\\xml\\order.xml").Root;
XElement? orderItems = XDocument.Load("..\\..\\..\\..\\xml\\orderItem.xml").Root;

//=========================== Order functions
/// <summary>
/// A function that receives details from the user about the new order and sends them to the function for adding an order.
/// </summary>
void AddOrder()
{
    XElement? root = XDocument.Load("..\\..\\..\\..\\xml\\config.xml").Root;
    Order newOrder = new Order();
    Console.WriteLine("\nEnter your details:\n name-");
    newOrder.CustomerName = Console.ReadLine();
    Console.WriteLine("\nemail-");
    newOrder.CustomerEmail = Console.ReadLine();
    Console.WriteLine("\naddress-");
    newOrder.CustomerAddress = Console.ReadLine();

    newOrder.Id = Convert.ToInt32(root.Element("MaxOrderId").Value.ToString());
    root.Element("MaxOrderId").Value = Convert.ToString(newOrder.Id + 1);
    root?.Save("..\\..\\..\\..\\xml\\config.xml");
    newOrder.OrderDate = DateTime.Now;
    newOrder.ShipDate = null;
    newOrder.DeliveryDate = null;
    dalXml.order.Create(newOrder);
}
/// <summary>
/// A function that receives details from the user for displaying order data, and sends them to the function to do so.
/// </summary>
void WatchOrder()
{
    Console.WriteLine("\nEnter the order ID for watching: ");
    int oId = Convert.ToInt32(Console.ReadLine());
    Console.WriteLine(dalXml.order.Read(oId) + "\n");
}
/// <summary>
/// A function that receives details from the user for displaying the data of all orders, and sends them to the function that will do it.
/// </summary>
void WatchOrderList()
{
    IEnumerable<Order> ordList = dalXml.order.Read();
    foreach (Order order in ordList)
    {
        Console.WriteLine(order);
    }
}
/// <summary>
/// A function that receives details from the user for updating an order, and sends them to the function that will do it.
/// </summary>
void UpdateOrder()
{
    DateTime oDate, shipDate, deliveryDate;
    Order tmpOrder = new Order();

    Console.WriteLine("\nEnter the order id you want to update: ");
    tmpOrder.Id = Convert.ToInt32(Console.ReadLine());
    Order srcOrd = dalXml.order.Read(tmpOrder.Id);
    Console.WriteLine(srcOrd + "\n");
    Console.WriteLine("\nEnter your details:\n name- ");
    tmpOrder.CustomerName = Console.ReadLine();
    Console.WriteLine("\nemail- ");
    tmpOrder.CustomerEmail = Console.ReadLine();
    Console.WriteLine("\naddress- ");
    tmpOrder.CustomerAddress = Console.ReadLine();
    oDate = srcOrd.OrderDate;
    tmpOrder.OrderDate = oDate;
    Console.WriteLine("\nEnter the order shipping date: ");

    bool correctInput = false;
    correctInput = DateTime.TryParse(Console.ReadLine(), out shipDate);
    if (!correctInput)
    {
        throw new Exception("\nYou have entered an incorrect shipDate.");
    }
    tmpOrder.ShipDate = shipDate;
    Console.WriteLine("\nEnter the order delivering date: ");
    correctInput = DateTime.TryParse(Console.ReadLine(), out deliveryDate);
    if (!correctInput)
    {
        throw new Exception("\nYou have entered an incorrect deliveryDate.");
    }
    tmpOrder.DeliveryDate = deliveryDate;

    dalXml.order.Update(tmpOrder);
}
/// <summary>
/// A function that receives details from the user for deleting an order, and sends them to the function that will do it.
/// </summary>
void DeleteOrder()
{
    Console.WriteLine("\nEnter the ID of the order you want to delete: ");
    int oId = Convert.ToInt32(Console.ReadLine());
    dalXml.order.Delete(oId);
}
/// <summary>
/// A function that receives details from the user for displaying all the items in a specific order, and sends them to the function that will do it.
/// </summary>
void WatchAllItemsInOrd()
{
    Console.WriteLine("\nEnter the order ID you want to watch: ");
    int oId = Convert.ToInt32(Console.ReadLine());
    IEnumerable<OrderItem> itemsInOrder = dalXml.orderItem.Read((OrderItem => OrderItem.OrderId == oId));
    foreach (OrderItem oItem in itemsInOrder)
    {
        Console.WriteLine(oItem);
    }
}

//==================================== Product functions
/// <summary>
/// A function that receives details from the user about the new product and sends them to the function for adding an product.
/// </summary>
void AddProduct()
{
    Product newProduct = new Product();

    Console.WriteLine("Enter the product details:\n name-");
    newProduct.Name = Console.ReadLine();
    Console.WriteLine("\ncategory- ");
    newProduct.Category = (eCategory)Convert.ToInt32(Console.ReadLine());
    Console.WriteLine("\nprice- ");
    newProduct.Price = Convert.ToDouble(Console.ReadLine());
    Console.WriteLine("\ninStock- ");
    newProduct.InStock = Convert.ToInt32(Console.ReadLine());

    bool notExists;
    int id;
    do      //Rand id and make sure is unique in the array 
    {
        notExists = true;
        Random rnd = new Random();
        id = rnd.Next(100000, 1000000);
        List<XElement> productsXml = products.Elements("Product").ToList();
        List<Product> productsList = new();
        foreach (XElement productXml in productsXml)
        {
            Product prd = new();
            prd.Id = Convert.ToInt32(productXml.Element("Id").Value.ToString());
            prd.Name = productXml.Element("Name").Value.ToString();
            prd.Price = Convert.ToDouble(productXml.Element("Price").Value.ToString());
            prd.InStock = Convert.ToInt32(productXml.Element("InStock").Value.ToString());
            prd.Category = (eCategory)Enum.Parse(typeof(eCategory), productXml.Element("Category").Value.ToString());
            productsList.Add(prd);
        }
        for (int j = 0; j < productsList.Count; j++)
        {
            if (productsList[j].Id == id)
            {
                notExists = false;
                break;
            }
        }
    } while (!notExists);
    newProduct.Id = id;
    dalXml.product.Create(newProduct);
}
/// <summary>
/// A function that receives details from the user for updating an product, and sends them to the function that will do it.
/// </summary>.
void UpdateProduct()
{
    Product tmpProduct = new Product();

    Console.WriteLine("\nEnter the product's details you want to update:\n id- ");
    tmpProduct.Id = Convert.ToInt32(Console.ReadLine());
    Product srcProd = dalXml.product.Read(tmpProduct.Id);
    Console.WriteLine(srcProd + "\n");
    Console.WriteLine("\nname- ");
    tmpProduct.Name = Console.ReadLine();
    Console.WriteLine("\ncategory- ");
    tmpProduct.Category = (eCategory)Convert.ToInt32(Console.ReadLine());
    Console.WriteLine("\ninStock- ");
    tmpProduct.InStock = Convert.ToInt32(Console.ReadLine());
    Console.WriteLine("\nprice- ");
    tmpProduct.Price = Convert.ToDouble(Console.ReadLine());
    dalXml.product.Update(tmpProduct);
}
/// <summary>
/// A function that receives details from the user for displaying product data, and sends them to the function to do so.
/// </summary>
void WatchProduct()
{
    Console.WriteLine("\nEnter the product ID for watching: ");
    int id = Convert.ToInt32(Console.ReadLine());
    Product tmpProduct = dalXml.product.Read(id);
    Console.WriteLine(tmpProduct + "\n");
}
/// <summary>
/// A function that receives details from the user for displaying the data of all products, and sends them to the function that will do it.
/// </summary>
void WatchProductList()
{
    int size = products.Elements("Product").Count();
    IEnumerable<Product> productList = dalXml.product.Read();
    foreach (Product product in productList)
    {
        Console.WriteLine(product);
    }
}
/// <summary>
/// A function that receives details from the user for deleting a product, and sends them to the function that will do it.
/// </summary>
void DeleteProduct()
{
    Console.WriteLine("\nEnter the ID of the product you want to delete: ");
    int id = Convert.ToInt32(Console.ReadLine());
    dalXml.product.Delete(id);
}


//============================================= OrderItem functions
/// <summary>
/// A function that receives details from the user about the new item in order and sends them to the function for adding an item to an existing or new order.
/// </summary>
void AddOrderItem()
{
    OrderItem newOrderItem = new OrderItem();
    int productIdxInList = -1;
    bool correct;
    Console.WriteLine("Enter OrderItem details:");

    List<XElement> productsXml = (List<XElement>)products.Elements("Product").ToList();
    List<Product> productList = new();
    foreach (XElement productXml in productsXml)
    {
        Product prd = new();
        prd.Id = Convert.ToInt32(productXml.Element("Id").Value.ToString());
        prd.Name = productXml.Element("Name").Value.ToString();
        prd.Price = Convert.ToDouble(productXml.Element("Price").Value.ToString());
        prd.InStock = Convert.ToInt32(productXml.Element("InStock").Value.ToString());
        prd.Category = (eCategory)Enum.Parse(typeof(eCategory), productXml.Element("Category").Value.ToString());
        productList.Add(prd);
    }

    XmlRootAttribute xRoot = new XmlRootAttribute();
    xRoot.ElementName = "Orders";
    xRoot.IsNullable = true;
    StreamReader ordersRead = new StreamReader("..\\..\\..\\..\\xml\\order.xml");
    XmlSerializer ser = new(typeof(List<DO.Order>), xRoot);
    List<DO.Order> orderList = (List<DO.Order>?)ser.Deserialize(ordersRead);
    ordersRead.Close();

    do
    {
        correct = false;
        Console.WriteLine("productId-");
        newOrderItem.ProductId = Convert.ToInt32(Console.ReadLine());
        for (int j = 0; j < productList.Count; j++)//checking that this productId exists 
        {
            if (productList[j].Id == newOrderItem.ProductId)
            {
                correct = true;
                productIdxInList = j;
            }
        }
        if (!correct)
            Console.WriteLine("this productId doesn't exist");
    } while (!correct);
    do
    {
        correct = false;
        Console.WriteLine("orderId-");
        newOrderItem.OrderId = Convert.ToInt32(Console.ReadLine());
        for (int j = 0; j < orderList.Count; j++)//checking that this orderId exists 
        {
            if (orderList[j].Id == newOrderItem.OrderId)
            {
                correct = true;
            }
        }
        if (!correct)
            Console.WriteLine("this orderId doesn't exist");
    } while (!correct);
    XElement? root = XDocument.Load("..\\..\\..\\..\\xml\\config.xml").Root;
    Console.WriteLine("amount-");
    newOrderItem.Amount = Convert.ToInt32(Console.ReadLine());
    newOrderItem.Price = (productList[productIdxInList].Price);
    newOrderItem.Id = Convert.ToInt32(root.Element("MaxOrderItemId").Value.ToString());
    root.Element("MaxOrderItemId").Value = Convert.ToString(newOrderItem.Id + 1);
    root?.Save("..\\..\\..\\..\\xml\\config.xml");
    dalXml.orderItem.Create(newOrderItem);
}
/// <summary>
/// A function that receives details from the user for updating an item (by ID) in the order, and sends them to the function that will do it.
/// </summary>
void UpdateOrderItem()
{
    OrderItem newOrderItem = new OrderItem();
    int productIdxInList = -1;
    bool correct;
    Console.WriteLine("\nEnter the orderItem's details you want to update:\n id- ");
    newOrderItem.Id = Convert.ToInt32(Console.ReadLine());
    OrderItem tmpOrdItem = dalXml.orderItem.Read(newOrderItem.Id);
    Console.WriteLine(tmpOrdItem + "\n");

    List<XElement> productsXml = (List<XElement>)products.Elements("Product").ToList();
    List<Product> productList = new();
    foreach (XElement productXml in productsXml)
    {
        Product prd = new();
        prd.Id = Convert.ToInt32(productXml.Element("Id").Value.ToString());
        prd.Name = productXml.Element("Name").Value.ToString();
        prd.Price = Convert.ToDouble(productXml.Element("Price").Value.ToString());
        prd.InStock = Convert.ToInt32(productXml.Element("InStock").Value.ToString());
        prd.Category = (eCategory)Enum.Parse(typeof(eCategory), productXml.Element("Category").Value.ToString());
        productList.Add(prd);
    }

    XmlRootAttribute xRoot = new XmlRootAttribute();
    xRoot.ElementName = "Orders";
    xRoot.IsNullable = true;
    StreamReader ordersRead = new StreamReader("..\\..\\..\\..\\xml\\order.xml");
    XmlSerializer ser = new(typeof(List<DO.Order>), xRoot);
    List<DO.Order> orderList = (List<DO.Order>?)ser.Deserialize(ordersRead);
    ordersRead.Close();

    do
    {
        correct = false;
        Console.WriteLine("productId-");
        newOrderItem.ProductId = Convert.ToInt32(Console.ReadLine());
        for (int j = 0; j < productList.Count; j++)//checking that this productId exists 
        {
            if (productList[j].Id == newOrderItem.ProductId)
            {
                correct = true;
                productIdxInList = j;
            }
        }
        if (!correct)
            Console.WriteLine("this productId doesn't exist");
    } while (!correct);
    do
    {
        correct = false;
        Console.WriteLine("orderId-");
        newOrderItem.OrderId = Convert.ToInt32(Console.ReadLine());
        for (int j = 0; j < orderList.Count; j++)//checking that this orderId exists 
        {
            if (orderList[j].Id == newOrderItem.OrderId)
            {
                correct = true;
            }
        }
        if (!correct)
            Console.WriteLine("this orderId doesn't exist");
    } while (!correct);
    Console.WriteLine("amount-");
    newOrderItem.Amount = Convert.ToInt32(Console.ReadLine());
    newOrderItem.Price = (productList[productIdxInList].Price);
    dalXml.orderItem.Update(newOrderItem);
}
/// <summary>
/// A function that receives details from the user for displaying an item in the order (by the item ID), 
/// and sends them to the function that will do it.
/// </summary>
void WatchOrderItem()
{
    Console.WriteLine("\nEnter the orderItem ID for watching: ");
    int id = Convert.ToInt32(Console.ReadLine());
    OrderItem tmpOrderItem = dalXml.orderItem.Read(id);
    Console.WriteLine(tmpOrderItem + "\n");
}
/// <summary>
/// A function that receives details from the user for displaying the data of all items of allthe existing orders, and sends them to the function that will do it.
/// </summary>
void WatchOrderItemList()
{
    IEnumerable<OrderItem> orderItemList = dalXml.orderItem.Read();
    foreach (OrderItem orderItem in orderItemList)
    {
        Console.WriteLine(orderItem);
    }
}
/// <summary>
/// A function that receives details from the user for deleting a item in an order, and sends them to the function that will do it.
/// </summary>
void DeleteOrderItem()
{
    Console.WriteLine("\nEnter the ID of the orderItem you want to delete: ");
    int id = Convert.ToInt32(Console.ReadLine());
    dalXml.orderItem.Delete(id);
}
/// <summary>
/// A function that receives details from the user for displaying an item in the order (by the order ID and the product ID), 
/// and sends them to the function that will do it.
/// </summary>
void WatchOrderItemByOrderIdProductId()
{
    Console.WriteLine("\nEnter the order ID : ");
    int oId = Convert.ToInt32(Console.ReadLine());
    Console.WriteLine("\nEnter the product ID : ");
    int pId = Convert.ToInt32(Console.ReadLine());
    OrderItem tmpOrderItem = dalXml.orderItem.ReadSingle((OrderItem => (OrderItem.OrderId == oId && OrderItem.ProductId == pId)));
    Console.WriteLine(tmpOrderItem + "\n");
}


/// <summary>
/// A function that presents the user with a menu of actions for the selected entity, receives a action by  
/// input and routes it to the appropriate function.
/// </summary>
void Menue(string type)
{
    string specialOptions = ".";
    if (type == "orderItem")
    {
        specialOptions = $@"f for watching all items in a specific order 
            g for watching a specific item of a specific order.";
    }
    if (type == "order")
    {
        specialOptions = $@"f for watching all the items in the order
            g for watching a specific item of the order.";
    }
    Console.WriteLine($@"
            Choose the following action:
            a for adding an {type},
            b for watching an {type},
            c for watching the {type}s list,
            d for updating an {type},
            e for deleting an {type} from the {type}s list
            {specialOptions}
            ");

    choice = Console.ReadKey().KeyChar;
    if (type == "order")
    {
        switch (choice)
        {
            case 'a':   // add
                AddOrder();
                break;
            case 'b':
                WatchOrder();
                break;
            case 'c':
                WatchOrderList();
                break;
            case 'd':
                UpdateOrder();
                break;
            case 'e':
                DeleteOrder();
                break;
            case 'f':
                WatchAllItemsInOrd();
                break;
            case 'g':
                WatchOrderItemByOrderIdProductId();
                break;
            default:
                throw new Exception("unknown action");
                break;
        }
    }
    if (type == "product")
    {
        switch (choice)
        {
            case 'a':   // add
                AddProduct();
                break;
            case 'b':
                WatchProduct();
                break;
            case 'c':
                WatchProductList();
                break;
            case 'd':
                UpdateProduct();
                break;
            case 'e':
                DeleteProduct();
                break;
            default:
                throw new Exception("unknown action");
                break;

        }
    }
    if (type == "orderItem")
    {
        switch (choice)
        {
            case 'a':   // add
                AddOrderItem();
                break;
            case 'b':
                WatchOrderItem();
                break;
            case 'c':
                WatchOrderItemList();
                break;
            case 'd':
                UpdateOrderItem();
                break;
            case 'e':
                DeleteOrderItem();
                break;
            case 'f':
                WatchAllItemsInOrd();
                break;
            case 'g':
                WatchOrderItemByOrderIdProductId();
                break;
            default:
                throw new Exception("unknown action");
        }
    }
}

void main()
{
    bool toContinue = true;
    while (toContinue)
    {
        try
        {
            Console.WriteLine(" Press 0 to exit,\n" +
            " 1 to check the orders list,\n " +
            " 2 to check the products list,\n" +
            " 3 to check the orders items list. ");
            choice = Console.ReadKey().KeyChar;
            switch (choice)
            {
                case '0':
                    toContinue = false;
                    break;
                case '1':
                    Menue("order");
                    break;
                case '2':
                    Menue("product");
                    break;
                case '3':
                    Menue("orderItem");
                    break;
            }
        }
        catch (Exception errMsg)
        {
            Console.WriteLine(errMsg.Message + "\n");
        }
    }
}

main();