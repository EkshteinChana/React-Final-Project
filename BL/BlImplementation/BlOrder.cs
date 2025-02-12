﻿using BlApi;
using DalApi;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

namespace BlImplementation;
internal class BlOrder : BlApi.IOrder
{
    private IDal? Dal = DalApi.Factory.Get();

   
    [MethodImpl(MethodImplOptions.Synchronized)]
    BO.Order BlApi.IOrder.ReadOrd(int orderId)
    {

        if (orderId < 0)
        {
            throw new InvalidValueException("ID");
        }
        try
        {
            lock (Dal)
            {
                DO.Order dO = Dal.order.Read(orderId);
                return convertDToB(dO);
            }
        }
        catch (IdNotExistException err)
        {
            throw new DataErrorException(err, "Data Error: ");
        }

    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    IEnumerable<BO.OrderForList> BlApi.IOrder.ReadOrdsManager()
    {
        lock (Dal)
        {
            IEnumerable<DO.Order> dOrders = Dal.order.Read();

            List<BO.OrderForList> orderList = new List<BO.OrderForList>(dOrders.Count());
            var enumerator = dOrders.OrderBy(dO => dO.Id).GetEnumerator();

            while (enumerator.MoveNext())
            {
                BO.Order bO = convertDToB(enumerator.Current);
                BO.OrderForList ordForList = new();
                ordForList.GetType().GetProperties().Where(prop => prop.Name != "AmountOfItems").Select(prop =>
                {
                    prop.SetValue(ordForList, bO.GetType().GetProperty(prop.Name)?.GetValue(bO));
                    return prop;
                }).ToList();
                ordForList.AmountOfItems = bO.Items.Count();
                orderList.Add(ordForList);
            }
            return orderList;
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    BO.Order BlApi.IOrder.UpdateOrd(int oId, int pId, int amount, BO.eUpdateOrder action) // (Bonus)
    {
        try
        {
            lock (Dal)
            {
                DO.Order dOrd = Dal.order.Read(oId);
                IEnumerable<DO.OrderItem> orderItems = Dal.orderItem.Read();

                if (dOrd.OrderDate == null)
                {
                    throw new IllegalActionException("The customer has not completed the order.");
                }
                if (dOrd.ShipDate != null)
                {
                    throw new IllegalActionException("The order has already been sent.");
                }

                DO.OrderItem? itmInOrd = orderItems.Where(oItm => (oItm.ProductId == pId && oItm.OrderId == oId)).FirstOrDefault();

                switch (action)
                {
                    case BO.eUpdateOrder.add:
                        if (!itmInOrd.Equals(default(DO.OrderItem)))
                        {
                            throw new IllegalActionException("Adding a product that already exists in the order.");
                        }
                        DO.OrderItem newItm = new();
                        //for xml
                        XElement? root = XDocument.Load(@"..\xml\config.xml").Root;
                        newItm.Id = Convert.ToInt32(root.Element("MaxOrderItemId").Value.ToString());
                        root.Element("MaxOrderItemId").Value = Convert.ToString(newItm.Id + 1);
                        root?.Save(@"..\xml\config.xml");
                        //for list
                        //newItm.Id = DataSource.Config.MaxOrderItemId;
                        newItm.OrderId = oId;
                        newItm.ProductId = pId;
                        newItm.Price = Dal.product.Read(pId).Price;
                        newItm.Amount = amount;
                        Dal.orderItem.Create(newItm);
                        break;
                    case BO.eUpdateOrder.delete:
                        if (itmInOrd.Equals(default(DO.OrderItem)) || itmInOrd == null)
                        {
                            throw new IllegalActionException("Deletion of a product that does not exist in the order.");
                        }
                        DO.OrderItem tmpItmInOrd = (DO.OrderItem)itmInOrd;
                        Dal.orderItem.Delete(tmpItmInOrd.Id);
                        break;
                    case BO.eUpdateOrder.changeAmount:
                        if (itmInOrd.Equals(default(DO.OrderItem)) || itmInOrd == null)
                        {
                            throw new IllegalActionException("Updating the quantity of an item that does not exist in the order");
                        }
                        DO.OrderItem tmpItmInOrd1 = (DO.OrderItem)itmInOrd;
                        tmpItmInOrd1.Amount = amount;
                        Dal.orderItem.Update(tmpItmInOrd1);
                        break;
                }
                return convertDToB(dOrd);
            }
        }
        catch (IdNotExistException err)
        {
            throw new DataErrorException(err, "Data Error: ");
        }
        catch (IdAlreadyExistsException err)
        {
            throw new DataErrorException(err, "Data Error: ");
        }

    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    BO.Order BlApi.IOrder.UpdateOrdDelivery(int oId)
    {
        if (oId < 0)
        {
            throw new InvalidValueException("ID");
        }
        try
        {
            lock (Dal)
            {
                DO.Order dOrder = Dal.order.Read(oId);

                if (dOrder.DeliveryDate != null)
                {
                    throw new IllegalActionException("The order has already been delivered.");
                }
                if (dOrder.ShipDate == null)
                {
                    throw new IllegalActionException("The order has not been sent yet.");
                }
                dOrder.DeliveryDate = DateTime.Now;
                Dal.order.Update(dOrder);
                return convertDToB(dOrder);
            }
        }
        catch (IdNotExistException err)
        {
            throw new DataErrorException(err, "Data Error: ");
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    BO.Order BlApi.IOrder.UpdateOrdShipping(int oId)
    {

        if (oId < 0)
        {
            throw new InvalidValueException("ID");
        }
        try
        {
            lock (Dal)
            {
                DO.Order dOrder = Dal.order.Read(oId);
                if (dOrder.ShipDate != null)
                {
                    throw new IllegalActionException("The order has already been sent.");
                }
                dOrder.ShipDate = DateTime.Now;
                Dal.order.Update(dOrder);
                return convertDToB(dOrder);
            }
        }
        catch (IdNotExistException err)
        {
            throw new DataErrorException(err, "Data Error: ");
        }

    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public BO.OrderTracking TrackOrder(int orderId)
    {
        lock (Dal)
        {
            if (orderId < 0)
            {
                throw new InvalidValueException("ID");
            }
            DO.Order order = new();
            try
            {
                order = Dal.order.Read(orderId);
                BO.OrderTracking ot = new()
                {
                    Id = order.Id,
                    status = order.ShipDate == null ? BO.eOrderStatus.confirmed : order.DeliveryDate == null ? BO.eOrderStatus.Sent : BO.eOrderStatus.provided,
                    OrderStatusByDate = new()
                };
                if (order.OrderDate != null)
                {
                    ot.OrderStatusByDate.Add(new Tuple<DateTime?, BO.eOrderStatus>(order.OrderDate, BO.eOrderStatus.confirmed));
                    if (order.ShipDate != null)
                    {
                        ot.OrderStatusByDate.Add(new Tuple<DateTime?, BO.eOrderStatus>(order.ShipDate, BO.eOrderStatus.Sent));
                        if (order.DeliveryDate != null)
                            ot.OrderStatusByDate.Add(new Tuple<DateTime?, BO.eOrderStatus>(order.DeliveryDate, BO.eOrderStatus.provided));
                    }
                }
                return ot;
            }
            catch (IdNotExistException err)
            {
                throw new DataErrorException(err, "Data Error: ");
            }
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int? GetOldestOrder()
    {

        DateTime? oldestDate = DateTime.MaxValue;
        int id = 0;
        lock (Dal)
        {
            IEnumerable<DO.Order> dOrders = Dal.order.Read();

            List<BO.OrderForList> orderList = new List<BO.OrderForList>(dOrders.Count());
            var enumerator = dOrders.OrderBy(dO => dO.Id).GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.ShipDate == null)
                {
                    if (enumerator.Current.OrderDate < oldestDate)
                    {
                        oldestDate = enumerator.Current.OrderDate;
                        id = enumerator.Current.Id;
                    }
                }
                else if (enumerator.Current.DeliveryDate == null)
                {
                    if (enumerator.Current.ShipDate < oldestDate)
                    {
                        oldestDate = enumerator.Current.ShipDate;
                        id = enumerator.Current.Id;
                    }
                }
            }
        }
        if (oldestDate != DateTime.MaxValue)
            return id;
        return null;
    }

    /// <summary>
    /// A private function to discover the status of the order.
    /// </summary>
    private BO.eOrderStatus checkStatus(DO.Order dO)
    {
        if (dO.DeliveryDate != null)
        {
            return BO.eOrderStatus.provided;
        }
        else if (dO.ShipDate != null)
        {
            return BO.eOrderStatus.Sent;
        }
        else
        {
            return BO.eOrderStatus.confirmed;
        }
    }

    /// <summary>
    /// A private function to convert DO.Order to BO.Order.
    /// </summary>
    private BO.Order convertDToB(DO.Order dO)
    {
        BO.Order bO = new();
        bO.GetType().GetProperties().Select(prop =>
        {
            prop.SetValue(bO, dO.GetType().GetProperty(prop.Name)?.GetValue(dO));
            return prop;
        }).ToList();
        bO.status = checkStatus(dO);
        //items and totalPrice:
        IEnumerable<DO.OrderItem> orderItems = Dal?.orderItem.Read() ?? Enumerable.Empty<DO.OrderItem>();
        IEnumerable<DO.OrderItem> items = new List<DO.OrderItem>(orderItems.Count());
        items = orderItems.Where(ordItm => ordItm.OrderId == bO.Id);
        bO.Items = new List<BO.OrderItem>(items.Count());
        List<BO.OrderItem?> bItemsList = bO.Items.ToList();
        bO.TotalPrice = 0;
        var enumerator = items.GetEnumerator();
        while (enumerator.MoveNext())
        {
            BO.OrderItem bItm = new();
            bItm.GetType().GetProperties().Where(prop => prop.Name != "OrderId").Select(prop =>
            {
                prop.SetValue(bItm, enumerator.Current.GetType().GetProperty(prop.Name)?.GetValue(enumerator.Current));
                return prop;
            }).ToList();
            bItm.Name = (Dal.product.Read(enumerator.Current.ProductId)).Name;
            bItm.TotalPrice = bItm.Price * bItm.Amount;
            bO.TotalPrice += bItm.TotalPrice;
            bItemsList.Add(bItm);
        }
        bO.Items = bItemsList.OrderBy(x => x.Id);
        return bO;
    }

}


