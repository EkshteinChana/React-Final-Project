﻿using BlApi;

using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace PL.Orders;

/// <summary>
/// Interaction logic for OrderListWindow.xaml
/// </summary>
public partial class OrderListWindow : Window
{
    private IBl bl;
    private ObservableCollection<PO.OrderForList?> currentOrderList { get; set; }//the list of the orders 
    /// <summary>
    /// A private help function to convert BO.OrderForList entity to PO.OrderForList entity.
    /// </summary>
    private PO.OrderForList convertBoOrdLstToPoOrdLst(BO.OrderForList bO)
    {
        PO.OrderForList po = new()
        {
            Id = bO.Id,
            CustomerName = bO.CustomerName,
            AmountOfItems = bO.AmountOfItems,
            TotalPrice = bO.TotalPrice
        };
        if (bO.status == BO.eOrderStatus.confirmed) { po.status = PO.eOrderStatus.confirmed; }
        else if (bO.status == BO.eOrderStatus.provided) { po.status = PO.eOrderStatus.provided; }
        else { po.status = PO.eOrderStatus.Sent; }
        return po;
    }
    /// <summary>
    /// constractor of OrderListWindow which imports the list of orders.
    /// </summary>
    public OrderListWindow(IBl Ibl)
    {
        InitializeComponent();
        bl = Ibl;
        IEnumerable<BO.OrderForList?> bOrds = bl.Order.ReadOrdsManager();
        currentOrderList = new();
        bOrds?.Select(bO =>
        {
            PO.OrderForList o = convertBoOrdLstToPoOrdLst(bO);
            currentOrderList.Add(o);
            return bO;
        }).ToList();
        OrdersListview.DataContext = currentOrderList;
        Simulator.Simulator.registerChangeStatusEvent(refreshStatus);
    }
    /// <summary>
    /// A function that opens the OrderWindow for watching and updating the selected order.
    /// </summary>
    private void OrdersListview_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        PO.OrderForList O = (PO.OrderForList)((ListView)sender).SelectedItem;
        new OrderWindow(bl, this, O.Id, currentOrderList).Show();
        Hide();
    }
    /// <summary>
    /// A function for returning to the AdminWindow.
    /// </summary>
    private void ReturnBackBtn_Click(object sender, RoutedEventArgs e)
    {
        new PL.AdminWindow(bl).Show();
        Simulator.Simulator.unregisterChangeStatusEvent(refreshStatus);
        Close();
    }
    /// <summary>
    /// A function that update a order in the screan if its status changed in the simulator. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void refreshStatus(object sender, EventArgs e)
    {
        if (!CheckAccess())
        {
            Dispatcher.BeginInvoke(refreshStatus, sender, e);
        }
        else
        {
            //int? oId = (e as Simulator.Num)?.id ?? null;
            //PO.OrderForList order = currentOrderList.Where(order => order.Id == oId).FirstOrDefault();
            //if (order == null || order.Equals(default(PO.OrderForList)))
            //{
            //    return;
            //}
            IEnumerable<BO.OrderForList?> bOrds = bl.Order.ReadOrdsManager();
            currentOrderList = new();
            bOrds?.Select(bO =>
            {
                PO.OrderForList o = convertBoOrdLstToPoOrdLst(bO);
                currentOrderList.Add(o);
                return bO;
            }).ToList();
            OrdersListview.DataContext = currentOrderList;
            //order.status = (PO.eOrderStatus)((int)order.status + 1);
        }
    }
    private void OrdersListview_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
}



