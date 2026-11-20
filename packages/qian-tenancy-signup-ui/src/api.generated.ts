// 由 OpenAPI 契约生成；运行对应包的 contracts:generate。
export interface paths {
    "/api/signup/verifications": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H2b08b2df67fa18e644cdceb83644a74fed61a270f4a2029d4c016215a071c47b"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/signup/session": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["H35fdba65ca94d7f8369838eba06c726039d8e86f100c58e6455ac0369dcfa281"];
        put?: never;
        post: operations["H5eaae600d40b446d4138866d8fe63ab537c2c76e7ee4d1f9462dbd97676e555c"];
        delete: operations["H6cb7eed1a54ac7a662ff44934cab0c5f521328ffb3fb4ffe868ea4964bcede47"];
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/signup/code-availability": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["Hc00ba6c8499247b237a0bd0021879ebbe9be3d6f7c0f9db9492c5c70cd1a3aa3"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/signup/me/applications": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["Hcb11751edfe0b115fc9c3c8c4c78b80255cbafbce9de4e34017f3ea56c4c596d"];
        put?: never;
        post: operations["Ha2beb20fcb0a6f4117b0c76e8f3f5629e48f78db22c88798cdcf6eb05ce33681"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/signup/me/applications/{id}:withdraw": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H197419483f0561d65bb80f3dbf56e8b2d10f553fe1ffbc78827ce64a714f6e9a"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/signup/applications": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["H1bef30d64f0861eadf936c01a97b31416e5d00da80dff3d7291787346496b250"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/signup/applications/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["H351aedd37fcb9f3c2fad756b72a2b812af952b90846f7c96dd0144a459fdc3da"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/signup/applications/{id}:approve": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H21b9cc8b44bd544b061031ffef2204788e40b43b0a0c21abbcef7cb38d7d6ef0"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/signup/applications/{id}:reject": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["Ha788f63d739e8e50d5f8b0e905f1f1fea28589d117c82f7e88386059509f23a8"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/signup/applications/{id}:retry": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H928b9def47e80919b7b6334dcf69a871ed253f19f9e3628f10b939fc0a862f66"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/signup/meta/state-machine": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["Hfa10adccf93161829ae9521d679e3f851319bb515dc2fbe60d5918832c899615"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
}
export type webhooks = Record<string, never>;
export interface components {
    schemas: {
        H85232dc0c2fec1c076415b309d52baf0a83a0845f2d50e1a049066661b0fd681: {
            phone: string;
        };
        H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa: {
            type: string;
            title: string;
            status: string | number;
            detail?: string | null;
            instance?: string | null;
            code: string;
            message: string;
            trace_id: string;
            pointer?: string | null;
            allowed?: unknown[] | null;
            errors?: {
                code: string;
                message: string;
                pointer: string;
                allowed?: unknown[] | null;
            }[] | null;
        };
        H8cefcf38198c4af149845d12de0b25076ed529596412d92fbd65b24bb657bed2: {
            /** Format: uuid */
            id: string;
            /** Format: date-time */
            expiresAt: string;
            /** Format: date-time */
            resendAfter: string;
        };
        H05152e37240c1414f247bf36025c2ac855d191fe9dda09ac09c12a3d068922fc: {
            /** Format: uuid */
            verificationId: string;
            code: string;
        };
        H808d21015c0302c247f25fa09030d3935bd6321c56aadb289cac9f2b5e707309: {
            phone: string;
            existingAccount: boolean;
            displayName?: string | null;
        };
        Hdaa80fc8c372663a6dc4b78ce804f8feae34824d3047e725f322223306e81476: string;
        H669090718dca2baa621d1ccb8e6766f9e50c7eeb479bbbbf3e8aafd7b0800bda: {
            code: string;
            available: boolean;
            reason?: string | null;
        };
        H0c8f43c501fc205349b9fe55c00aa681c27f76844f16b16bc3e71eb1a15b5e28: {
            items: {
                /** Format: uuid */
                id: string;
                number: string;
                /** @enum {unknown} */
                status: "submitted" | "provisioning" | "approved" | "failed" | "rejected" | "withdrawn";
                companyName: string;
                code: string;
                size: string;
                contactName: string;
                phone: string;
                /** Format: date-time */
                submittedAt: string;
                industry?: string | null;
                description?: string | null;
                email?: string | null;
                /** Format: date-time */
                completedAt?: string | null;
                /** @enum {unknown} */
                plan?: "trial" | "formal" | null;
                /** Format: date-time */
                validUntil?: string | null;
                rejectReason?: string | null;
            }[];
        };
        H786626c6a2f2fb210efc579cfe132a631e73ffd95c88f30bcc3022c53a6f1bf0: {
            companyName: string;
            code: string;
            /** @enum {string} */
            size: "1-20" | "21-100" | "101-500" | "501-1000" | "1000+";
            /** @enum {boolean} */
            acceptTerms: true;
            displayName?: string | null;
            password?: string | null;
            email?: string | null;
            industry?: string | null;
            description?: string | null;
        };
        Hd176d4d6817c1851c9a508baee57da535702eb58619d4c8f9c68afd9b27026b1: {
            /** Format: uuid */
            id: string;
            number: string;
            /** @enum {unknown} */
            status: "submitted" | "provisioning" | "approved" | "failed" | "rejected" | "withdrawn";
            companyName: string;
            code: string;
            size: string;
            contactName: string;
            phone: string;
            /** Format: date-time */
            submittedAt: string;
            industry?: string | null;
            description?: string | null;
            email?: string | null;
            /** Format: date-time */
            completedAt?: string | null;
            /** @enum {unknown} */
            plan?: "trial" | "formal" | null;
            /** Format: date-time */
            validUntil?: string | null;
            rejectReason?: string | null;
        };
        /** Format: uuid */
        Hb5aa0e1df5fc81773384e1a45407eb45714901cfb28ca9f18f461e3d0edb6cfe: string;
        H3cd5af31c6c95a486a001dfd9dec3a13e8f187337fa374f3f725506fbe354d8c: string | number;
        H62d0b66f8b26775ad8721420239fb3e79fd1d107bbd232b127c8f9dcafb454cb: {
            items: {
                /** Format: uuid */
                id: string;
                number: string;
                /** @enum {unknown} */
                status: "submitted" | "provisioning" | "approved" | "failed" | "rejected" | "withdrawn";
                companyName: string;
                code: string;
                size: string;
                contactName: string;
                phone: string;
                /** Format: date-time */
                submittedAt: string;
                check: {
                    kind: string;
                    level: string;
                    reason?: string | null;
                    count?: string | number | null;
                } | null;
                industry?: string | null;
            }[];
            total: string | number;
            counts: {
                all: string | number;
                submitted: string | number;
                provisioning: string | number;
                approved: string | number;
                failed: string | number;
                rejected: string | number;
                withdrawn: string | number;
            };
        };
        Hb332c14a610826520b1cb4fb94091c97d3a998a99ba07b8d14fafb34aa14a5c5: {
            /** Format: uuid */
            id: string;
            number: string;
            /** @enum {unknown} */
            status: "submitted" | "provisioning" | "approved" | "failed" | "rejected" | "withdrawn";
            company: {
                name: string;
                code: string;
                size: string;
                industry?: string | null;
                description?: string | null;
            };
            applicant: {
                /** Format: uuid */
                subjectId: string;
                contactName: string;
                phone: string;
                existingAccount: boolean;
                email?: string | null;
            };
            checks: {
                kind: string;
                level: string;
                reason?: string | null;
                count?: string | number | null;
            }[];
            defaults: {
                trialDays: string | number;
                users: string | number;
            };
            /** Format: date-time */
            submittedAt: string;
            /** Format: date-time */
            completedAt?: string | null;
            review?: {
                reviewer: string;
                /** Format: date-time */
                reviewedAt: string;
                /** @enum {unknown} */
                plan?: "trial" | "formal" | null;
                /** Format: date-time */
                validUntil?: string | null;
                users?: string | number | null;
                rejectReason?: string | null;
                note?: string | null;
            } | null;
            provisioning?: {
                attempts: string | number;
                steps: {
                    step: string;
                    status: string;
                    attempts: string | number;
                    reason?: string | null;
                }[];
                /** Format: uuid */
                tenantId?: string | null;
                /** Format: uuid */
                membershipId?: string | null;
                failureReason?: string | null;
            } | null;
        };
        H89dbd52613c3d984be41906d96b383e158a41ce2785564907560dd004cdb87e5: {
            /** @enum {unknown} */
            plan: "trial" | "formal";
            code?: string | null;
            /** Format: date-time */
            validUntil?: string | null;
            users?: string | number | null;
            note?: string | null;
        };
        H721788222562263088c416fbf800f385897e9debf06a7d0170e77c9aac5ae1e8: {
            reason: string;
            note?: string | null;
        };
        H8a546ee3e06bbbbdd0266eea51c306c99b1f86b2a97071ff9d4e89f572fa09a2: {
            states: ("submitted" | "provisioning" | "approved" | "failed" | "rejected" | "withdrawn")[];
            terminal: ("submitted" | "provisioning" | "approved" | "failed" | "rejected" | "withdrawn")[];
            transitions: {
                from: ("submitted" | "provisioning" | "approved" | "failed" | "rejected" | "withdrawn")[];
                action: string;
                /** @enum {unknown} */
                to: "submitted" | "provisioning" | "approved" | "failed" | "rejected" | "withdrawn";
            }[];
            actions: {
                name: string;
                actor: string;
                sideEffect: string;
                agent: {
                    expose: boolean;
                    requiresConfirmation: boolean;
                };
            }[];
        };
    };
    responses: never;
    parameters: never;
    requestBodies: never;
    headers: never;
    pathItems: never;
}
export type $defs = Record<string, never>;
export interface operations {
    H2b08b2df67fa18e644cdceb83644a74fed61a270f4a2029d4c016215a071c47b: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H85232dc0c2fec1c076415b309d52baf0a83a0845f2d50e1a049066661b0fd681"];
            };
        };
        responses: {
            /** @description HTTP 202 */
            202: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H8cefcf38198c4af149845d12de0b25076ed529596412d92fbd65b24bb657bed2"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 415 */
            415: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 429 */
            429: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 503 */
            503: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    H35fdba65ca94d7f8369838eba06c726039d8e86f100c58e6455ac0369dcfa281: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H808d21015c0302c247f25fa09030d3935bd6321c56aadb289cac9f2b5e707309"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    H5eaae600d40b446d4138866d8fe63ab537c2c76e7ee4d1f9462dbd97676e555c: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H05152e37240c1414f247bf36025c2ac855d191fe9dda09ac09c12a3d068922fc"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H808d21015c0302c247f25fa09030d3935bd6321c56aadb289cac9f2b5e707309"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 415 */
            415: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    H6cb7eed1a54ac7a662ff44934cab0c5f521328ffb3fb4ffe868ea4964bcede47: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 204 */
            204: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    Hc00ba6c8499247b237a0bd0021879ebbe9be3d6f7c0f9db9492c5c70cd1a3aa3: {
        parameters: {
            query: {
                code: components["schemas"]["Hdaa80fc8c372663a6dc4b78ce804f8feae34824d3047e725f322223306e81476"];
            };
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H669090718dca2baa621d1ccb8e6766f9e50c7eeb479bbbbf3e8aafd7b0800bda"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 429 */
            429: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    Hcb11751edfe0b115fc9c3c8c4c78b80255cbafbce9de4e34017f3ea56c4c596d: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H0c8f43c501fc205349b9fe55c00aa681c27f76844f16b16bc3e71eb1a15b5e28"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    Ha2beb20fcb0a6f4117b0c76e8f3f5629e48f78db22c88798cdcf6eb05ce33681: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H786626c6a2f2fb210efc579cfe132a631e73ffd95c88f30bcc3022c53a6f1bf0"];
            };
        };
        responses: {
            /** @description HTTP 201 */
            201: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Hd176d4d6817c1851c9a508baee57da535702eb58619d4c8f9c68afd9b27026b1"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 415 */
            415: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 422 */
            422: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    H197419483f0561d65bb80f3dbf56e8b2d10f553fe1ffbc78827ce64a714f6e9a: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hb5aa0e1df5fc81773384e1a45407eb45714901cfb28ca9f18f461e3d0edb6cfe"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Hd176d4d6817c1851c9a508baee57da535702eb58619d4c8f9c68afd9b27026b1"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 415 */
            415: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    H1bef30d64f0861eadf936c01a97b31416e5d00da80dff3d7291787346496b250: {
        parameters: {
            query?: {
                status?: components["schemas"]["Hdaa80fc8c372663a6dc4b78ce804f8feae34824d3047e725f322223306e81476"];
                keyword?: components["schemas"]["Hdaa80fc8c372663a6dc4b78ce804f8feae34824d3047e725f322223306e81476"];
                size?: components["schemas"]["Hdaa80fc8c372663a6dc4b78ce804f8feae34824d3047e725f322223306e81476"];
                skip?: components["schemas"]["H3cd5af31c6c95a486a001dfd9dec3a13e8f187337fa374f3f725506fbe354d8c"];
                take?: components["schemas"]["H3cd5af31c6c95a486a001dfd9dec3a13e8f187337fa374f3f725506fbe354d8c"];
            };
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H62d0b66f8b26775ad8721420239fb3e79fd1d107bbd232b127c8f9dcafb454cb"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    H351aedd37fcb9f3c2fad756b72a2b812af952b90846f7c96dd0144a459fdc3da: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hb5aa0e1df5fc81773384e1a45407eb45714901cfb28ca9f18f461e3d0edb6cfe"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Hb332c14a610826520b1cb4fb94091c97d3a998a99ba07b8d14fafb34aa14a5c5"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    H21b9cc8b44bd544b061031ffef2204788e40b43b0a0c21abbcef7cb38d7d6ef0: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hb5aa0e1df5fc81773384e1a45407eb45714901cfb28ca9f18f461e3d0edb6cfe"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H89dbd52613c3d984be41906d96b383e158a41ce2785564907560dd004cdb87e5"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Hb332c14a610826520b1cb4fb94091c97d3a998a99ba07b8d14fafb34aa14a5c5"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    Ha788f63d739e8e50d5f8b0e905f1f1fea28589d117c82f7e88386059509f23a8: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hb5aa0e1df5fc81773384e1a45407eb45714901cfb28ca9f18f461e3d0edb6cfe"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H721788222562263088c416fbf800f385897e9debf06a7d0170e77c9aac5ae1e8"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Hb332c14a610826520b1cb4fb94091c97d3a998a99ba07b8d14fafb34aa14a5c5"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    H928b9def47e80919b7b6334dcf69a871ed253f19f9e3628f10b939fc0a862f66: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hb5aa0e1df5fc81773384e1a45407eb45714901cfb28ca9f18f461e3d0edb6cfe"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Hb332c14a610826520b1cb4fb94091c97d3a998a99ba07b8d14fafb34aa14a5c5"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
    Hfa10adccf93161829ae9521d679e3f851319bb515dc2fbe60d5918832c899615: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H8a546ee3e06bbbbdd0266eea51c306c99b1f86b2a97071ff9d4e89f572fa09a2"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H7225839678ce47ba7b6be7f4659020074a82cb096736bac9c2feb5eac85b8efa"];
                };
            };
        };
    };
}
