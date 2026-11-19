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
        H22efab4c86c47fc7012b78d6f2e3df8d842b5978e8ef1e9b5914ac04bde4cac6: {
            phone: string;
        };
        H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8: {
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
        Hd9be280780b899d0b445e741e51a4bb30d3163f5d857e564bf15eb7dee0d69db: {
            /** Format: uuid */
            id: string;
            /** Format: date-time */
            expiresAt: string;
            /** Format: date-time */
            resendAfter: string;
        };
        H68f39752b964068d1fc2099141fb45395ae1d0039d31d732fd4840738aba0e78: {
            /** Format: uuid */
            verificationId: string;
            code: string;
        };
        H9f13714e42df1e4f258a575a65c4cc0f30e93a09b45c268a801f07f2369e216a: {
            phone: string;
            existingAccount: boolean;
            displayName?: string | null;
        };
        Hd9eacec9f1b8feaf92c605e7d4531376b3994d3a184f18dbf002d8d54e37055c: string;
        H782453cc5324ca697694e14ad14374e63d626b194321371e92c5bbcc623fbce6: {
            code: string;
            available: boolean;
            reason?: string | null;
        };
        Hff3f6728d5f77bcfd4e912df0ca1c16e317569244ac7243390e5d2de6d313615: {
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
        H5e542b5defaa7a0a22f45b326c8509e50660af41ea9b8dac7c4d7054879ca783: {
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
        H46847a55c002f4d2702001d10d2152608c39d9fbd117157dbfee1598b83075c4: {
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
        H0ef39f8f8ea8707ab397a03addd15e846001a526b650ddb130f26af04cfe07d2: string;
        H5feca5d855beb5e05288db2b462eaf7479abb6c2272885e6031a3b5eb7b491c9: string | number;
        H582fc26b7e833b296044b8364de3d0e39cdf347fb58d41d36de7f37ee364ad04: {
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
        H4e4d89d0d6d8f36eba442bf3ed82df5bc16d1ecfa54f4c5684d5802d315648c4: {
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
        Hbe497f6931da12abfe0561fe18925efec2d86ad893e8e87f7cc0c09dddd027f4: {
            /** @enum {unknown} */
            plan: "trial" | "formal";
            code?: string | null;
            /** Format: date-time */
            validUntil?: string | null;
            users?: string | number | null;
            note?: string | null;
        };
        Hb41bb9df00f598f117d84997a1983f29038820f79633ade35041dd818fe46d80: {
            reason: string;
            note?: string | null;
        };
        H1a419bdea7867342a552f534c75a4f99140c7b50e48174d72de0517ded09a8ca: {
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
                "application/json": components["schemas"]["H22efab4c86c47fc7012b78d6f2e3df8d842b5978e8ef1e9b5914ac04bde4cac6"];
            };
        };
        responses: {
            /** @description HTTP 202 */
            202: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Hd9be280780b899d0b445e741e51a4bb30d3163f5d857e564bf15eb7dee0d69db"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 415 */
            415: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 429 */
            429: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 503 */
            503: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
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
                    "application/json": components["schemas"]["H9f13714e42df1e4f258a575a65c4cc0f30e93a09b45c268a801f07f2369e216a"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
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
                "application/json": components["schemas"]["H68f39752b964068d1fc2099141fb45395ae1d0039d31d732fd4840738aba0e78"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H9f13714e42df1e4f258a575a65c4cc0f30e93a09b45c268a801f07f2369e216a"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 415 */
            415: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
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
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
        };
    };
    Hc00ba6c8499247b237a0bd0021879ebbe9be3d6f7c0f9db9492c5c70cd1a3aa3: {
        parameters: {
            query: {
                code: components["schemas"]["Hd9eacec9f1b8feaf92c605e7d4531376b3994d3a184f18dbf002d8d54e37055c"];
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
                    "application/json": components["schemas"]["H782453cc5324ca697694e14ad14374e63d626b194321371e92c5bbcc623fbce6"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 429 */
            429: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
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
                    "application/json": components["schemas"]["Hff3f6728d5f77bcfd4e912df0ca1c16e317569244ac7243390e5d2de6d313615"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
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
                "application/json": components["schemas"]["H5e542b5defaa7a0a22f45b326c8509e50660af41ea9b8dac7c4d7054879ca783"];
            };
        };
        responses: {
            /** @description HTTP 201 */
            201: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H46847a55c002f4d2702001d10d2152608c39d9fbd117157dbfee1598b83075c4"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 415 */
            415: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 422 */
            422: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
        };
    };
    H197419483f0561d65bb80f3dbf56e8b2d10f553fe1ffbc78827ce64a714f6e9a: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H0ef39f8f8ea8707ab397a03addd15e846001a526b650ddb130f26af04cfe07d2"];
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
                    "application/json": components["schemas"]["H46847a55c002f4d2702001d10d2152608c39d9fbd117157dbfee1598b83075c4"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 415 */
            415: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
        };
    };
    H1bef30d64f0861eadf936c01a97b31416e5d00da80dff3d7291787346496b250: {
        parameters: {
            query?: {
                status?: components["schemas"]["Hd9eacec9f1b8feaf92c605e7d4531376b3994d3a184f18dbf002d8d54e37055c"];
                keyword?: components["schemas"]["Hd9eacec9f1b8feaf92c605e7d4531376b3994d3a184f18dbf002d8d54e37055c"];
                size?: components["schemas"]["Hd9eacec9f1b8feaf92c605e7d4531376b3994d3a184f18dbf002d8d54e37055c"];
                skip?: components["schemas"]["H5feca5d855beb5e05288db2b462eaf7479abb6c2272885e6031a3b5eb7b491c9"];
                take?: components["schemas"]["H5feca5d855beb5e05288db2b462eaf7479abb6c2272885e6031a3b5eb7b491c9"];
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
                    "application/json": components["schemas"]["H582fc26b7e833b296044b8364de3d0e39cdf347fb58d41d36de7f37ee364ad04"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
        };
    };
    H351aedd37fcb9f3c2fad756b72a2b812af952b90846f7c96dd0144a459fdc3da: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H0ef39f8f8ea8707ab397a03addd15e846001a526b650ddb130f26af04cfe07d2"];
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
                    "application/json": components["schemas"]["H4e4d89d0d6d8f36eba442bf3ed82df5bc16d1ecfa54f4c5684d5802d315648c4"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
        };
    };
    H21b9cc8b44bd544b061031ffef2204788e40b43b0a0c21abbcef7cb38d7d6ef0: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H0ef39f8f8ea8707ab397a03addd15e846001a526b650ddb130f26af04cfe07d2"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["Hbe497f6931da12abfe0561fe18925efec2d86ad893e8e87f7cc0c09dddd027f4"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H4e4d89d0d6d8f36eba442bf3ed82df5bc16d1ecfa54f4c5684d5802d315648c4"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
        };
    };
    Ha788f63d739e8e50d5f8b0e905f1f1fea28589d117c82f7e88386059509f23a8: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H0ef39f8f8ea8707ab397a03addd15e846001a526b650ddb130f26af04cfe07d2"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["Hb41bb9df00f598f117d84997a1983f29038820f79633ade35041dd818fe46d80"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H4e4d89d0d6d8f36eba442bf3ed82df5bc16d1ecfa54f4c5684d5802d315648c4"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
        };
    };
    H928b9def47e80919b7b6334dcf69a871ed253f19f9e3628f10b939fc0a862f66: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["H0ef39f8f8ea8707ab397a03addd15e846001a526b650ddb130f26af04cfe07d2"];
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
                    "application/json": components["schemas"]["H4e4d89d0d6d8f36eba442bf3ed82df5bc16d1ecfa54f4c5684d5802d315648c4"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
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
                    "application/json": components["schemas"]["H1a419bdea7867342a552f534c75a4f99140c7b50e48174d72de0517ded09a8ca"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H5b5b17f6ea59331331bb44bdb3f72215740afda9720e547162e19af8cf95e1f8"];
                };
            };
        };
    };
}
