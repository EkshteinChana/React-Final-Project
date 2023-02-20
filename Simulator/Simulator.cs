﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlApi;
using BlImplementation;
using BO;

namespace Simulator;

internal static class Simulator
{
    private static BlApi.IBl bl = BlApi.Factory.Get();
    private static Order currentOrder;
    private static bool continuing=true;
    public static void Run()
    {
        while(continuing)
        new Thread(() =>
        {
            try
            {
                int? id = bl.Order.GetOldestOrder();
                if (id == null)
                {
                    throw new Exception("No orders to update");
                }
                currentOrder = bl.Order.ReadOrd((int)id);
                Random rnd = new Random();
                int seconds = rnd.Next(1000, 5000);
                Thread.Sleep(seconds);
                if (currentOrder.status == eOrderStatus.confirmed)
                {
                    bl.Order.UpdateOrdShipping(currentOrder.Id);
                }
                else
                {
                    bl.Order.UpdateOrdDelivery(currentOrder.Id);
                }
            }
            catch (InvalidValueException err)
            {
                throw err;
            }
            catch (DataErrorException dataError)
            {
                throw dataError;
            }
            catch (Exception err)
            {
                throw err;
            }
        }).Start();
        

    }
    public static void Stop()
    {
        continuing=false;
    }


}

