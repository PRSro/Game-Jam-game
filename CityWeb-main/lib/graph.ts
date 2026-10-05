/**
 * Graph helpers for Piața - link-first traversal (Where/When/Who)
 * Everything connects through connections table
 */

export type ConnectionType = 
  | 'event_place'
  | 'person_event'
  | 'person_neighborhood'
  | 'job_place'
  | 'traffic_event'
  | 'poll_place'
  | 'business_place'
  | 'other'

export type NodeType = 'events' | 'places' | 'persons' | 'businesses' | 'job-ads' | 'traffic-disruptions' | 'polls' | 'neighborhoods'

export interface ConnectionEdge {
  id: string
  type: ConnectionType
  sourceType: NodeType
  source: unknown
  targetType: NodeType
  target: unknown
  weight: number
  metadata?: unknown
}

/**
 * Build graph query helpers - conceptual
 * Actual implementation depends on DB client
 */
export const GraphHelpers = {
  /**
   * Get all items connected to a source
   */
  getConnected: async (_sourceId: string, _sourceType: NodeType) => {
    // Implementation via connections table
    return []
  },
  
  /**
   * Get neighborhood connections for a person
   */
  getNeighborhoodForPerson: async (_personId: string) => {
    return []
  },
  
  /**
   * Get events affecting by traffic disruption
   */
  getAffectedEvents: async (_trafficId: string) => {
    return []
  },
}
