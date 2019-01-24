﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CWSStart.Web.Pocos;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web.Routing;

namespace CWSStart.Web.CWSExtensions
{

    public class RegisterStartupEvents : ApplicationEventHandler
    {
        private const int LabelDataTypeID = -92;
        private const int TextStringDataTypeID = -88;
        private const int BooleanDataTypeID = -49;
        private static object _lockObj = new object();
        private static bool _ran = false;

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Umbraco App has started

            // Taken from http://our.umbraco.org/wiki/reference/api-cheatsheet/using-iapplicationeventhandler-to-register-events
            // Could prevent this issue https://github.com/warrenbuckley/CWS-Start/issues/18
            // lock
            if (!_ran)
            {
                lock (_lockObj)
                {
                    if (!_ran)
                    {
                        // everything we do here is blocking
                        // on application start, so we should be 
                        // quick. 
                        // do you're registering here... or in a function 
                        // you will need to add the relevent class at the top of your code (i.e using Umbraco.cms.businesslogic.web)
                        //Ensure our custom member type & it's properties are setup in Umbraco
                        //If not let's create it
                        MemberGroup checkMemberGroup = MemberGroup.GetByName(ConfigHelper.GetCWSMemberGroupName());

                        //Doesn't exist
                        if (checkMemberGroup == null)
                        {
                            //Add custom member group to Umbraco install
                            AddCustomMemberGroup();
                        }

                        //Ensure our custom member type & it's properties are setup in Umbraco
                        //If not let's create it
                        MemberType checkMemberType = MemberType.GetByAlias(ConfigHelper.GetCWSMemberTypeAlias());

                        //Doesn't exist
                        if (checkMemberType == null)
                        {
                            //Add custom member type to Umbraco install
                            AddCustomMemberType();
                        }

                        //Get the Umbraco Database context
                        var db = applicationContext.DatabaseContext.Database;

                        //Check if the DB table does NOT exist
                        if (!db.TableExist("ContactFormLogs"))
                        {
                            //Create DB table - and set overwrite to false
                            db.CreateTable<ContactForm>(false);
                        }



                        //Create our custom MVC route for our member profile pages
                        RouteTable.Routes.MapRoute(
                            "memberProfileRoute",
                            "user/{profileURLtoCheck}",
                            new
                            {
                                controller = "ViewProfile",
                                action = "Index"
                            });
                        _ran = true;
                    }
                }
            }
        }

        protected void AddCustomMemberGroup()
        {
            //Admin user
            var adminUser = new User(0);

            //Let's add our Member Group
            var customMemberGroup = MemberGroup.MakeNew(ConfigHelper.GetCWSMemberGroupName(), adminUser);

            //Save it
            customMemberGroup.Save();
        }


        protected void AddCustomMemberType()
        {
            //Admin user
            var adminUser = new User(0);

            //So let's add it...
            var customMemberType = MemberType.MakeNew(adminUser, ConfigHelper.GetCWSMemberTypeAlias());
            customMemberType.Text = ConfigHelper.GetCWSMemberTypeName();
            customMemberType.Alias = ConfigHelper.GetCWSMemberTypeAlias();

            //Our DataType's (Have to be verbose with namespace, due to conflict with Umbraco.Core.Models)
            var labelDataType = new umbraco.cms.businesslogic.datatype.DataTypeDefinition(LabelDataTypeID);
            var textstringDataType = new umbraco.cms.businesslogic.datatype.DataTypeDefinition(TextStringDataTypeID);
            var boolDataType = new umbraco.cms.businesslogic.datatype.DataTypeDefinition(BooleanDataTypeID);

            //Add the custom properties to our member type
            //Labels
            customMemberType.AddPropertyType(labelDataType, "resetGUID", "Reset GUID");
            customMemberType.AddPropertyType(labelDataType, "emailVerifyGUID", "Email Verify GUID");
            customMemberType.AddPropertyType(labelDataType, "numberOfLogins", "Number of Logins");
            customMemberType.AddPropertyType(labelDataType, "lastLoggedIn", "Last logged in");
            customMemberType.AddPropertyType(labelDataType, "numberOfProfileViews", "Number of Profile views");
            customMemberType.AddPropertyType(labelDataType, "hostNameOfLastLogin", "Hostname of last login");
            customMemberType.AddPropertyType(labelDataType, "IPofLastLogin", "IP of last login");
            customMemberType.AddPropertyType(labelDataType, "profileURL", "Profile URL");
            customMemberType.AddPropertyType(labelDataType, "joinedDate", "Joined Date");

            //Booleans
            customMemberType.AddPropertyType(boolDataType, "hasVerifiedEmail", "Has Verified Email");

            //Textsring
            customMemberType.AddPropertyType(textstringDataType, "description", "Description");
            customMemberType.AddPropertyType(textstringDataType, "twitter", "Twitter");
            customMemberType.AddPropertyType(textstringDataType, "linkedIn", "LinkedIn");
            customMemberType.AddPropertyType(textstringDataType, "skype", "Skype");

            //Save the changes
            customMemberType.Save();
        }


        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //On application starting event...
            //Add to the ContentFinder resolver collection our custom 404 Content Finder resolver
            ContentLastChanceFinderResolver.Current.SetFinder(new _404iLastChanceFinder());
        }
    }
}