// 由 OpenAPI 契约生成；运行对应包的 contracts:generate。
export interface paths {
    "/api/tenancy/tenants": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["Hf958b36220b04fdafafd4ce1bf1cc7bb286010b9174b9603f521db51c2838ed4"];
        put?: never;
        post: operations["H32e213ade311e9ee4ad412d3b841e07f7eb4e5e83c4a8b4f27b75763a085a00e"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["H7282d39a5a69d6f1f24ed30a899bdd068a7ebf21ccdbff30f76bf9f0c76338a4"];
        put: operations["H0c94440aa57ef71f7214584848f8d938319335c6bc11651d69daea25551ccefb"];
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}:activate": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["Hfb5635e6cfff70e8a4c5e59b16dd6743501530f0b6c26b47a6b08fa844dc414b"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}:suspend": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["Hd9c36e685d5005ed061faf9b8c30a0874c23b55e72d1465dc922c2f84fc8d7c4"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}:resume": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H4a21ef0f599db0f5482691b331fb045074833c31fe972f1e6d5a222803e665f5"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/meta/state-machine": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["H0baeea7de8416226f77a77b251f92c9889405ba0e854f1a75fbf5aaa3d605c95"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}/settings/{plugin}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["H6e4552e1952535ae2b6d11e52db4e4104a4f85170191ae9715d76c2577f5474a"];
        put: operations["H14c3651c8980a91cd510dc051dcc0866c8a3a316587cbc581ccecbb0e34f56f8"];
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}/quota": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["Hb102428e0a5eb1cef582378078f5c7bc09c86bfa196cb477a9d4478538804837"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/admin-accounts:lookup": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H8ecda9a6b94f4dcf0b3ba3123d1744496aa689c21706099504d4933e7c93d8b7"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/admin-accounts": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H2de9f42fa23f4a129a730895f52190bdee4fccb3123c68063553938cd1b4a48d"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}/admins": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["Hf6d90ba241fbb8aad901e2660704d4710a1d2af4fd9601f2c4c35a899c8fe1e8"];
        put: operations["Hb4391e639d6f2aecd6af5b7150e5e8b15e77103b0895c71d03742630e6f9248d"];
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}/admins/{membershipId}:reset-password": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: operations["H0d50c6458dc0456a17b1778ac4e2fdfec0daf83542a0f7d3420682f66f73fdde"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/tenancy/tenants/{id}/summary": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: operations["H0dad2d4f1c2a907936436caac9a9aa704088f11b213a0b40148bcec73440aeee"];
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
        H676cf4854cbbb64f4fcdb3ee21a8266e842d3a839a46c271339745712d05cf31: string | number;
        H720036e1146a8fcaff6d0ca6a8345c0cc8c7e6862e54433cc04580dac409932d: string;
        H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10: {
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
        H4c3df786d64f865da0dfd26577fef9ddd8654c5d9f9d44408e98f0413266c75c: {
            items: {
                /** Format: uuid */
                id: string;
                code: string;
                name: string;
                /** @enum {unknown} */
                isolation: "row" | "schema" | "database";
                /** @enum {unknown} */
                status: "pending" | "trial" | "active" | "grace" | "suspended";
                revision?: string | number;
                /** Format: date-time */
                createdAt?: string;
                /** Format: date-time */
                validUntil?: string | null;
                usersLimit?: string | number | null;
                usersUsed?: string | number | null;
                administratorName?: string | null;
            }[];
            total?: string | number;
            counts?: {
                all: string | number;
                pending: string | number;
                trial: string | number;
                active: string | number;
                grace: string | number;
                suspended: string | number;
            };
        };
        H63d4cbd5380a661df13c6466d4b71989697c175247d323a354e856fbdc927da8: {
            code: string;
            name: string;
            /**
             * @default row
             * @enum {unknown}
             */
            isolation: "row";
            administrator: {
                /** Format: uuid */
                membershipId: string;
                subjectId: string;
                initialPassword?: string | null;
            };
            quotas?: {
                /** @default null */
                users: string | number | null;
            } | null;
        };
        H3f995015f749fc516eb6034ba00faf8d2047ee2af44c63dee2c529fe613d0b14: {
            /** Format: uuid */
            id: string;
            code: string;
            name: string;
            /** @enum {unknown} */
            isolation: "row" | "schema" | "database";
            /** @enum {unknown} */
            status: "pending" | "trial" | "active" | "grace" | "suspended";
            revision?: string | number;
        };
        /** Format: uuid */
        Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494: string;
        H68f65dc3da1cc19757c858d47e3c460e95643aad682ec75710c7de3b6ad71431: {
            name: string;
            expectedRevision: string | number;
            quotas?: {
                /** @default null */
                users: string | number | null;
            } | null;
        };
        Hc849b230da08366eb3b3272f6a643c08af6c9fdf9c862481510741ef05725668: {
            /** @default false */
            trial: boolean;
            /**
             * Format: date-time
             * @default null
             */
            validUntil: string | null;
        };
        /** 租户状态机元数据 */
        H591ab0ca13d5f0bee30459b9d177be3b8ada88c3bfb037965f829113e19a8b4d: {
            states: ("pending" | "trial" | "active" | "grace" | "suspended")[];
            transitions: {
                from: ("pending" | "trial" | "active" | "grace" | "suspended" | "any-s1" | "previous") | ("pending" | "trial" | "active" | "grace" | "suspended")[];
                action: string;
                to: ("pending" | "trial" | "active" | "grace" | "suspended" | "any-s1" | "previous") | ("pending" | "trial" | "active" | "grace" | "suspended")[];
            }[];
            actions: {
                name: string;
                /** @enum {unknown} */
                sideEffect: "read" | "write" | "destructive";
                agent: {
                    expose: boolean;
                    requiresConfirmation: boolean;
                };
                available?: boolean;
                stage?: string;
                idempotent?: boolean;
            }[];
        };
        Hdaabba13be8b58969e92d8876b5665b0790de9dd5d2ed65dfa05d59536e987d4: {
            pluginId: string;
            entryId: string;
            revision: string | number;
            config: unknown;
        };
        H53f955210cb5c5c4ea0682c621ae8dc5f5252b912739bfe43d993a742dad72bb: {
            config: unknown;
            expectedRevision: string | number;
        };
        Ha92537160da50fe7c4452a5cd0c25bfc0c91f0ae567d697f7544182cbbc53f97: {
            limits: {
                /** @default null */
                users: string | number | null;
            };
            usersUsed: string | number | null;
            usageAvailable: boolean;
        };
        H5a9cff06515bc34dc6ee6c5965c7e26bcb0f7de66f2acb833c71255fc83e8995: {
            identifier: string;
        };
        H453a0be767ab20c51d36e411ad118c5ca2a8042d559e2b2ca6e38278018c4bed: {
            /** Format: uuid */
            subjectId: string;
            displayName: string;
            status: string;
            identifiers: {
                /** @enum {unknown} */
                kind: "username" | "phone" | "email";
                masked: string;
                verified: boolean;
            }[];
        };
        H5ac54f3995d1aaf73a1b05e38e9f45e490a7a7c90c0aaf28346cdf45bf816aa1: {
            displayName: string;
            /** @enum {unknown} */
            identifierKind: "username" | "phone" | "email";
            identifier: string;
            initialPassword: string;
        };
        Hd32c5c778d7ba34f24ce550a5fbcccc3941a22cec36af28be001e6b1711e2818: {
            items: {
                /** Format: uuid */
                membershipId: string;
                /** Format: uuid */
                subjectId: string;
                displayName: string;
                account: string;
                status: string;
                identifiers: {
                    /** @enum {unknown} */
                    kind: "username" | "phone" | "email";
                    masked: string;
                    verified: boolean;
                }[];
            }[];
        };
        H9e2d9f0f1df21c960e2ecc28497608ea9abdf5c91d0b55d873461ea65dd9e751: {
            items: {
                /** Format: uuid */
                subjectId: string;
                initialPassword?: string | null;
            }[];
        };
        H89be34233fda50f839f309cf5ef41240ddfb2db2915cf4c3d048fba46b18161c: {
            newPassword: string;
        };
        H43afbf962482e97cdd70b383a00aa619e6b48b6fa33f2e7c4e21b96b3d98d555: {
            /** Format: uuid */
            id: string;
            code: string;
            name: string;
            /** @enum {unknown} */
            status: "pending" | "trial" | "active" | "grace" | "suspended";
            /** Format: date-time */
            validUntil?: string | null;
            usersLimit?: string | number | null;
            usersUsed?: string | number | null;
            administratorName?: string | null;
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
    Hf958b36220b04fdafafd4ce1bf1cc7bb286010b9174b9603f521db51c2838ed4: {
        parameters: {
            query?: {
                skip?: components["schemas"]["H676cf4854cbbb64f4fcdb3ee21a8266e842d3a839a46c271339745712d05cf31"];
                take?: components["schemas"]["H676cf4854cbbb64f4fcdb3ee21a8266e842d3a839a46c271339745712d05cf31"];
                status?: components["schemas"]["H720036e1146a8fcaff6d0ca6a8345c0cc8c7e6862e54433cc04580dac409932d"];
                keyword?: components["schemas"]["H720036e1146a8fcaff6d0ca6a8345c0cc8c7e6862e54433cc04580dac409932d"];
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
                    "application/json": components["schemas"]["H4c3df786d64f865da0dfd26577fef9ddd8654c5d9f9d44408e98f0413266c75c"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    H32e213ade311e9ee4ad412d3b841e07f7eb4e5e83c4a8b4f27b75763a085a00e: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H63d4cbd5380a661df13c6466d4b71989697c175247d323a354e856fbdc927da8"];
            };
        };
        responses: {
            /** @description HTTP 201 */
            201: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H3f995015f749fc516eb6034ba00faf8d2047ee2af44c63dee2c529fe613d0b14"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    H7282d39a5a69d6f1f24ed30a899bdd068a7ebf21ccdbff30f76bf9f0c76338a4: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
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
                    "application/json": components["schemas"]["H3f995015f749fc516eb6034ba00faf8d2047ee2af44c63dee2c529fe613d0b14"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    H0c94440aa57ef71f7214584848f8d938319335c6bc11651d69daea25551ccefb: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H68f65dc3da1cc19757c858d47e3c460e95643aad682ec75710c7de3b6ad71431"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H3f995015f749fc516eb6034ba00faf8d2047ee2af44c63dee2c529fe613d0b14"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    Hfb5635e6cfff70e8a4c5e59b16dd6743501530f0b6c26b47a6b08fa844dc414b: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["Hc849b230da08366eb3b3272f6a643c08af6c9fdf9c862481510741ef05725668"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H3f995015f749fc516eb6034ba00faf8d2047ee2af44c63dee2c529fe613d0b14"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    Hd9c36e685d5005ed061faf9b8c30a0874c23b55e72d1465dc922c2f84fc8d7c4: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
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
                    "application/json": components["schemas"]["H3f995015f749fc516eb6034ba00faf8d2047ee2af44c63dee2c529fe613d0b14"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    H4a21ef0f599db0f5482691b331fb045074833c31fe972f1e6d5a222803e665f5: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
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
                    "application/json": components["schemas"]["H3f995015f749fc516eb6034ba00faf8d2047ee2af44c63dee2c529fe613d0b14"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    H0baeea7de8416226f77a77b251f92c9889405ba0e854f1a75fbf5aaa3d605c95: {
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
                    "application/json": components["schemas"]["H591ab0ca13d5f0bee30459b9d177be3b8ada88c3bfb037965f829113e19a8b4d"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    H6e4552e1952535ae2b6d11e52db4e4104a4f85170191ae9715d76c2577f5474a: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
                plugin: components["schemas"]["H720036e1146a8fcaff6d0ca6a8345c0cc8c7e6862e54433cc04580dac409932d"];
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
                    "application/json": components["schemas"]["Hdaabba13be8b58969e92d8876b5665b0790de9dd5d2ed65dfa05d59536e987d4"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    H14c3651c8980a91cd510dc051dcc0866c8a3a316587cbc581ccecbb0e34f56f8: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
                plugin: components["schemas"]["H720036e1146a8fcaff6d0ca6a8345c0cc8c7e6862e54433cc04580dac409932d"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H53f955210cb5c5c4ea0682c621ae8dc5f5252b912739bfe43d993a742dad72bb"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Hdaabba13be8b58969e92d8876b5665b0790de9dd5d2ed65dfa05d59536e987d4"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    Hb102428e0a5eb1cef582378078f5c7bc09c86bfa196cb477a9d4478538804837: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
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
                    "application/json": components["schemas"]["Ha92537160da50fe7c4452a5cd0c25bfc0c91f0ae567d697f7544182cbbc53f97"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    H8ecda9a6b94f4dcf0b3ba3123d1744496aa689c21706099504d4933e7c93d8b7: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H5a9cff06515bc34dc6ee6c5965c7e26bcb0f7de66f2acb833c71255fc83e8995"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H453a0be767ab20c51d36e411ad118c5ca2a8042d559e2b2ca6e38278018c4bed"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    H2de9f42fa23f4a129a730895f52190bdee4fccb3123c68063553938cd1b4a48d: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H5ac54f3995d1aaf73a1b05e38e9f45e490a7a7c90c0aaf28346cdf45bf816aa1"];
            };
        };
        responses: {
            /** @description HTTP 201 */
            201: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["H453a0be767ab20c51d36e411ad118c5ca2a8042d559e2b2ca6e38278018c4bed"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 422 */
            422: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    Hf6d90ba241fbb8aad901e2660704d4710a1d2af4fd9601f2c4c35a899c8fe1e8: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
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
                    "application/json": components["schemas"]["Hd32c5c778d7ba34f24ce550a5fbcccc3941a22cec36af28be001e6b1711e2818"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    Hb4391e639d6f2aecd6af5b7150e5e8b15e77103b0895c71d03742630e6f9248d: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H9e2d9f0f1df21c960e2ecc28497608ea9abdf5c91d0b55d873461ea65dd9e751"];
            };
        };
        responses: {
            /** @description HTTP 200 */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Hd32c5c778d7ba34f24ce550a5fbcccc3941a22cec36af28be001e6b1711e2818"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 422 */
            422: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    H0d50c6458dc0456a17b1778ac4e2fdfec0daf83542a0f7d3420682f66f73fdde: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
                membershipId: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["H89be34233fda50f839f309cf5ef41240ddfb2db2915cf4c3d048fba46b18161c"];
            };
        };
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
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 422 */
            422: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
    H0dad2d4f1c2a907936436caac9a9aa704088f11b213a0b40148bcec73440aeee: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["schemas"]["Hc557b2f5f1d90291647f890f3ee918abeb70c26dac6226fc84f231c0d84b3494"];
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
                    "application/json": components["schemas"]["H43afbf962482e97cdd70b383a00aa619e6b48b6fa33f2e7c4e21b96b3d98d555"];
                };
            };
            /** @description HTTP 400 */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 401 */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 403 */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 404 */
            404: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
            /** @description HTTP 409 */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["H4fa60c07d235cc10258e3c9ebc5a056d1a8a6370838cda0589e5f7278cc85a10"];
                };
            };
        };
    };
}
