export interface User {
  id: string;
  userName: string;
  email?: string;
  roles: string[];
}

export interface AuthResponse {
  id: string;
  userName: string;
  accessToken: string;
  expiresIn: number;
  roles: string[];
}

export interface Tier {
  id: string;
  name: string;
  color: string;
  order: number;
}

export interface TierItem {
  id: string;
  imageUrl: string;
  caption?: string;
  tierId?: string | null;
  position: number;
}

export interface TierListBrief {
  id: string;
  title: string;
  description?: string;
  authorId: string;
  authorUserName: string;
  templateId?: string | null;
  templateTitle?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  itemCount?: number;
  itemsCount?: number;
  tiersCount?: number;
  likes?: number;
  dislikes?: number;
  score?: number;
  likesCount?: number;
  dislikesCount?: number;
}

export interface TierListDetail {
  id: string;
  title: string;
  description?: string;
  authorId: string;
  authorUserName: string;
  templateId?: string | null;
  templateTitle?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  tiers: Tier[];
  items: TierItem[];
  likes?: number;
  dislikes?: number;
  score?: number;
  likesCount?: number;
  dislikesCount?: number;
  userVote?: number | null;
}

export interface TemplateTier {
  id: string;
  name: string;
  color: string;
  order: number;
}

export interface TemplateItem {
  id: string;
  imageUrl: string;
  caption?: string;
  order: number;
}

export interface TemplateBrief {
  id: string;
  authorId: string;
  authorUserName: string;
  title: string;
  description?: string;
  visibility: number; // 0 = Public, 1 = Unlisted
  itemCount: number;
  createdAt: string;
}

export interface TemplateDetail {
  id: string;
  authorId: string;
  authorUserName: string;
  title: string;
  description?: string;
  visibility: number;
  tiers: TemplateTier[];
  items: TemplateItem[];
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateTierRequest {
  id?: string;
  name: string;
  color: string;
  order: number;
}

export interface CreateTierItemRequest {
  id?: string;
  imageUrl: string;
  caption?: string;
}

export interface CreateTierListRequest {
  title: string;
  description?: string;
  templateId?: string;
  tiers?: CreateTierRequest[];
  items?: CreateTierItemRequest[];
  layout?: SaveLayoutRequest;
}

export interface CreateTemplateTierModel {
  name: string;
  color: string;
  order: number;
}

export interface CreateTemplateItemModel {
  imageUrl: string;
  caption?: string;
  order: number;
}

export interface CreateTemplateRequest {
  title: string;
  description?: string;
  visibility: number;
  tiers: CreateTemplateTierModel[];
  items: CreateTemplateItemModel[];
}

export interface TierLayoutItem {
  tierId: string;
  itemIds: string[];
}

export interface SaveLayoutRequest {
  tiers: TierLayoutItem[];
  unplacedItemIds: string[];
}

export interface Comment {
  id: string;
  tierListId: string;
  authorId: string;
  authorUserName: string;
  text: string;
  createdAt: string;
  isDeleted: boolean;
  likesCount: number;
  dislikesCount: number;
  userVote?: number | null;
  parentCommentId?: string | null;
  replies?: Comment[];
}

export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface VoteResult {
  likesCount: number;
  dislikesCount: number;
  userVote?: number | null;
}

export interface UserRoleInfo {
  id: string;
  userName: string;
  email?: string;
  roles: string[];
  createdAt: string;
}
