using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TestHarness
{
    class Program
    {   // Propety name
        const string InformationAttributeTypeName = "UTILITYFUNCTIONS.INFORMATIONATTRIBUTE"; 

        static void Main(string[] args)
        { 
            // target assembly file name, and namespace of classes for testing
            const string TargetAssemblyFileName = "UtilityFunctions.dll";
            const string TargetNamespace = "UtilityFunctions";
            // load target assembly at runtime into memory
            // return value is object of type Assembly
            Assembly assembly = Assembly.LoadFile(Path.Combine(
                                            AppDomain.CurrentDomain.BaseDirectory,
                                            TargetAssemblyFileName));
            // using Linq to query a list of types or classes in the target assembly namespace
            // and convert the IEnumerable generic collection to array
            // those classes has custom attribute decoration
            List<System.Type> classes = assembly.GetTypes()
                .Where(t => t.Namespace == TargetNamespace && HasInformationAttribute(t)).ToList();

            while (true)
            {
                Console.Clear();
                WritePromptToScreen("Please press the number key associated with " +
                                    "the class you wish to test");

                // display all classes in the target assembly
                DisplayProgramElementList(classes);
                // choose one class from the classes list to test
                Type typeChoice = ReturnProgramElementReferenceFromList(classes);
                // Late bind a class within the target assemble
                // and instansiate the reference(not type safety, prone error)
                object classInstance = Activator.CreateInstance(typeChoice, null);
                // one class is choosen. 
                Console.Clear();

                WriteHeadingToScreen($"Class: '{typeChoice}'");

                // query the attribute of class and print attribute Property description 
                DisplayElementDescription(ReturnInformationCustomAttributeDescription(typeChoice));



                //// methods
                WritePromptToScreen("Please enter the number associated with the method you wish to test");

                // get all the methods in the choosen class
                // get those methods has attribute
                List<MethodInfo> methods = typeChoice.GetMethods().Where(m => HasInformationAttribute(m)).ToList();


                // diaplay a list of methods in the choosen class
                DisplayProgramElementList(methods); 
                // one method is choosen from methods list
                MethodInfo methodChoice = ReturnProgramElementReferenceFromList(methods);

                if (methodChoice != null)
                {
                    Console.Clear();

                    WriteHeadingToScreen($"Class: '{typeChoice}' - Method: '{methodChoice.Name}'");

                    // query the attribute of method and print attribute Property description
                    DisplayElementDescription(ReturnInformationCustomAttributeDescription(methodChoice));


                    //// get all the params of the method to array
                    ParameterInfo[] parameters = methodChoice.GetParameters();

                    // calculate the result by input argument in the method
                    object result = GetResult(classInstance, methodChoice, parameters);

                    WriteResultToScreen(result);


                }
                Console.WriteLine();
                WritePromptToScreen("Please press the 'Spacebar' key to end the application " +
                    "or any other key to continue...");

                if (Console.ReadKey().Key == ConsoleKey.Spacebar) break;

            }

        }
        private static string ReturnInformationCustomAttributeDescription(MemberInfo mi)
        {
            const string InformationAttributeDescriptionPropertyName = "Description";

            foreach (var attrib in mi.GetCustomAttributes())
            {
                Type typeAttrib = attrib.GetType();

                if (typeAttrib.ToString().ToUpper() == InformationAttributeTypeName)
                {
                    PropertyInfo propertyInfo = typeAttrib.GetProperty(InformationAttributeDescriptionPropertyName);
                    if (propertyInfo != null)
                    {
                        object s = propertyInfo.GetValue(attrib, null);

                        if (s != null)
                            return s.ToString();
                    }
                }

            }
            return null;

        }

        private static void DisplayElementDescription(string elementDescription)
        {
            if (elementDescription != null)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(elementDescription);
                Console.ResetColor();
                Console.WriteLine();
            }
        }


        // filter out custom attribute class
        private static bool HasInformationAttribute(MemberInfo mi)
        {
            // methodInfo and System.Type are in inherit from MemberInfo
            foreach (var attrib in mi.GetCustomAttributes())
            {
                Type typeAttrib = attrib.GetType();

                if (typeAttrib.ToString().ToUpper() == InformationAttributeTypeName)
                {
                    return true;
                }

            }
            return false;
        }

        private static void WriteResultToScreen(object result)
        {
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Result: {result}");
            Console.ResetColor();
            Console.WriteLine();
        }
        private static object[] ReturnParameterValueInputAsObjectArray(ParameterInfo[] parameters)
        {
            object[] paramValues = new object[parameters.Length];
            int itemCount = 0;

            foreach (ParameterInfo parameterInfo in parameters)
            {

                WritePromptToScreen($"Please enter a value for the parameter named, '{parameterInfo.Name}'");

                if (parameterInfo.ParameterType == typeof(string))
                {
                    string inputString = Console.ReadLine();
                    paramValues[itemCount] = inputString;
                }
                else if (parameterInfo.ParameterType == typeof(int))
                {
                    int inputInt = Int32.Parse(Console.ReadLine());
                    paramValues[itemCount] = inputInt;
                }
                else if (parameterInfo.ParameterType == typeof(double))
                {
                    double inputDouble = Double.Parse(Console.ReadLine());
                    paramValues[itemCount] = inputDouble;
                }

                itemCount++;

            }
            return paramValues;
        }
        private static object GetResult(Object classInstance, MethodInfo methodInfo,
                                        ParameterInfo[] parameters)
        {
            object result = null;

            if (parameters.Length == 0)
            {
                // first param is a reference of the instansiate  choosen class
                // null is passed into the method(no parameter)
                result = methodInfo.Invoke(classInstance, null);
            }
            else
            {
                // prompt user for method params
                var paramValueArray = ReturnParameterValueInputAsObjectArray(parameters);
                result = methodInfo.Invoke(classInstance, paramValueArray);
            }
            return result;
        }

        private static T ReturnProgramElementReferenceFromList<T>(List<T> list)
        {
            ConsoleKey consoleKey = Console.ReadKey().Key;

            switch (consoleKey)
            {
                case ConsoleKey.D1:
                    return list[0];
                case ConsoleKey.D2:
                    return list[1];
                case ConsoleKey.D3:
                    return list[2];
                case ConsoleKey.D4:
                    return list[3];

            }
            return default;
        }

        private static void DisplayProgramElementList<T>(List<T> list)
        {
            int count = 0;

            foreach (var item in list)
            {
                count++;
                Console.WriteLine($"{count}. {item}");
            }

        }

        private static void WriteHeadingToScreen(string heading)
        {
            Console.WriteLine(heading);
            Console.WriteLine(new string('-', heading.Length));
            Console.WriteLine();

        }

        private static void WritePromptToScreen(string promptText)
        {
            Console.WriteLine(promptText);
        }
    }
}