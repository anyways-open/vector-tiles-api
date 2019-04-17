using System;
using System.Collections.Generic;
using System.Linq;
using Itinero;
using Itinero.LocalGeo;
using Itinero.VectorTiles;
using Itinero.VectorTiles.Layers;
using Newtonsoft.Json.Linq;
using Attribute = Itinero.Attributes.Attribute;

namespace Anyways.VectorTiles.CycleNetworks
{
    public static class ConfigCreator
    {
        /// <summary>
        /// Creates a JSON-file which can be hosted with metadata.
        /// The term 'source' is very confusing here!
        /// </summary>
        /// <returns></returns>
        public static VectorTileSource CreateVectorSourceFor(this JObject obj,
            int minZoom, int maxZoom, Box box)
        {
            var layers = new List<VectorLayer>();

            foreach (var l in (JArray) obj["layers"])
            {
                if (!(l is JObject layer))
                {
                    throw new FormatException("Layers contains a non-object value");
                }

                layers.Add(new VectorLayer
                {
                    maxzoom = maxZoom,
                    minzoom = minZoom,
                    description = layer.GetFlatValue("description"),
                    id = layer.GetFlatValue("id")
                });
            }


            return new VectorTileSource
            {
                id = obj.GetFlatValue("id"),
                basename = obj.GetFlatValue("basename"),
                name = obj.GetFlatValue("name"),
                description = obj.GetFlatValue("description"),
                bounds = new double[] {box.MinLon, box.MinLat, box.MaxLon, box.MaxLat},
                maxzoom = maxZoom,
                minzoom = minZoom,
                vector_layers = layers.ToArray()
            };
        }

        /// <summary>
        /// Creates a VectorTileConfiguration based on the routerdb, ready to create all the actual tiles
        /// </summary>
        /// <param name="routerDb"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static VectorTileConfig CreateVectorTileConfigFor(this RouterDb routerDb, JObject obj)
        {
            var edgeLayerConfigs = new List<EdgeLayerConfig>();
            var vertexLayerConfigs = new List<VertexLayerConfig>();

            foreach (var l in (JArray) obj["layers"])
            {
                if (!(l is JObject layer))
                {
                    throw new FormatException("Layers contains a non-object value");
                }

                if (layer.GetFlatValue("type").Equals("node"))
                {
                    vertexLayerConfigs.Add(layer.CreateVertexLayerConfig(routerDb));
                }
                else if (layer.GetFlatValue("type").Equals("way"))
                {
                    edgeLayerConfigs.Add(layer.CreateEdgeLayerConfig(routerDb));
                }
                else
                {
                    throw new ArgumentException(
                        $"Invalid value for 'type' in layer: should be 'node' or 'way'\nError in layer: {layer}");
                }
            }

            var config = new VectorTileConfig
            {
                EdgeLayerConfigs = edgeLayerConfigs,
                VertexLayerConfigs = vertexLayerConfigs
            };

            return config;
        }

        private static VertexLayerConfig CreateVertexLayerConfig(this JObject j, RouterDb routerDb)
        {
            if (j["type"].ToString().ToLower() != "node")
            {
                throw new FormatException("Expected layer should have type 'way'");
            }

            var filter = j["filter"].CreatePredicate();

            return new VertexLayerConfig
            {
                Name = j.GetFlatValue("id"),
                GetAttributesFunc = (edgeId, zoom) =>
                {
                    var attributes = routerDb.GetVertexAttributes(edgeId)?.ToList();
                    if (attributes == null) return null;
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (!filter(attributes)) return null;
                    return attributes;
                }
            };
        }

        private static EdgeLayerConfig CreateEdgeLayerConfig(this JObject j, RouterDb routerDb)
        {
            if (j["type"].ToString().ToLower() != "way")
            {
                throw new FormatException("Expected layer should have type 'node'");
            }

            var filter = j["filter"].CreatePredicate();

            return new EdgeLayerConfig
            {
                Name = j.GetFlatValue("id"),
                GetAttributesFunc = (edgeId, zoom) =>
                {
                    var attributes = routerDb.GetEdgeAttributes(edgeId)?.ToList();
                    if (attributes == null) return null;

                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (!filter(attributes)) return null;
                    return attributes;
                }
            };
        }

        public static string GetFlatValue(this JObject j, string field)
        {
            if (!(j[field] is JValue))
            {
                throw new FormatException($"Field {field} should be a value");
            }

            return j[field].ToString();
        }

        private static Predicate<IEnumerable<Attribute>> CreatePredicate(this JToken obj)
        {
            if (!(obj is JObject j))
            {
                throw new FormatException("A filter should be an expression");
            }

            return j.CreatePredicate();
        }

        private static Predicate<IEnumerable<Attribute>> CreatePredicate(this JObject obj)
        {
            if (obj.ContainsKey("key") && obj.ContainsKey("value"))
            {
                var allowedValues = new List<string>();
                if (obj["value"] is JArray arr)
                {
                    foreach (var v in arr)
                    {
                        if (!(v is JValue))
                        {
                            throw new FormatException(
                                "The 'value' field should either contain values or a single value");
                        }

                        allowedValues.Add(v.ToString());
                    }
                }
                else
                {
                    allowedValues.Add(obj.GetFlatValue("value"));
                }


                var key = obj.GetFlatValue("key");
                return attributes =>
                {
                    return attributes.Any(a => a.Key.Equals(key) && allowedValues.Contains(a.Value));
                };
            }

            if (obj.ContainsKey("key"))
            {
                var key = obj.GetFlatValue("key");
                return attributes => { return attributes.Any(a => a.Key.Equals(key)); };
            }


            if (obj.ContainsKey("not"))
            {
                if (!(obj["not"] is JObject nObj))
                {
                    throw new FormatException("Invalid type: 'not' takes an expression");
                }

                var nt = CreatePredicate(nObj);
                return attributes => !nt(attributes);
            }

            if (obj.ContainsKey("and"))
            {
                if (!(obj["and"] is JArray ands))
                {
                    throw new FormatException("Invalid type: 'and' takes an array");
                }

                var exprs = new List<Predicate<IEnumerable<Attribute>>>();
                foreach (var e in ands)
                {
                    if (!(e is JObject expr))
                    {
                        throw new FormatException("Invalid type: 'and'-elements should be expressions");
                    }

                    exprs.Add(CreatePredicate(expr));
                }

                return attributes => exprs.All(predicate => predicate(attributes));
            }

            if (obj.ContainsKey("or"))
            {
                if (!(obj["or"] is JArray ands))
                {
                    throw new FormatException("Invalid type: 'or' takes an array");
                }

                var exprs = new List<Predicate<IEnumerable<Attribute>>>();
                foreach (var e in ands)
                {
                    if (!(e is JObject expr))
                    {
                        throw new FormatException("Invalid type: 'or'-elements should be expressions");
                    }

                    exprs.Add(CreatePredicate(expr));
                }

                return attributes => exprs.Any(predicate => predicate(attributes));
            }


            throw new FormatException($"Unknown expression: {obj}");
        }
    }
}